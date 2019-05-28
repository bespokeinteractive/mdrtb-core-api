using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using EtbSomalia.Extensions;
using EtbSomalia.Helpers;
using EtbSomalia.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EtbSomalia.Services
{
    public interface ICoreService {
        User Authenticate(string username, string password);
        Contacts GetContact(string uuid);
        List<Contacts> GetContacts(string p = "");
    }

    public class CoreService : ICoreService {
        private readonly AppSettings settings;

        public CoreService(IOptions<AppSettings> isettings) {
            settings = isettings.Value;
        }

        public User Authenticate(string username, string password) {
            var user = GetUser(username);
            if (user == null)
                return null;
            if (!new CrytoUtilsExtensions().Decrypt(user.Password).Equals(password))
                return null;
            if (!user.Enabled || user.ToChange)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(settings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.Name.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
            user.Password = null;

            return user;
        }

        private User GetUser(string username) {
            User user = null;

            SqlServerConnection conn = new SqlServerConnection(settings);
            SqlDataReader dr = conn.SqlServerConnect("SELECT log_username, usr_uuid, usr_name, usr_email, log_tochange, log_enabled, log_password FROM Login INNER JOIN Users ON log_user=usr_idnt WHERE log_username='" + username + "'");
            if (dr.Read()) {
                user = new User {
                    Username = dr[0].ToString(),
                    Uuid = dr[1].ToString(),
                    Name = dr[2].ToString(),
                    Email = dr[3].ToString(),
                    ToChange = Convert.ToBoolean(dr[4]),
                    Enabled = Convert.ToBoolean(dr[5]),
                    Password = dr[6].ToString()
                };
            }

            return user;
        }

        public Contacts GetContact(string uuid) {
            Contacts contact = null;

            SqlServerConnection conn = new SqlServerConnection(settings);
            SqlDataReader dr = conn.SqlServerConnect("SELECT ct_uuid, ct_identifier, ct_notes, ct_exposed_from, ct_added_on, ct_next_screening, cs.cpt_name [status], cl.cpt_name [location], cr.cpt_name [relation], cp.cpt_name [proximity], cd.cpt_name[disease_after], ct.cpt_name[previously_treated], p.ps_uuid, p.ps_name, p.ps_gender, p.ps_dob, pp_tbmu, pp_enrolled_on, pt_uuid, pt_dead, ps.ps_uuid, ps.ps_name, ps.ps_gender, ps.ps_dob, pt_death_date FROM Contacts INNER JOIN Person p ON ct_person=p.ps_idnt INNER JOIN PatientProgram ON ct_index=pp_idnt INNER JOIN Patient ON pp_patient=pt_idnt INNER JOIN Person ps ON pt_person=ps.ps_idnt INNER JOIN Concept cs ON ct_status= cs.cpt_id INNER JOIN Concept cl ON ct_location= cl.cpt_id INNER JOIN Concept cr ON ct_relationship= cr.cpt_id INNER JOIN Concept cp ON ct_proximity=cp.cpt_id INNER JOIN Concept cd ON ct_desease_after=cd.cpt_id INNER JOIN Concept ct ON ct_prev_treated=ct.cpt_id WHERE ct_uuid COLLATE SQL_Latin1_General_CP1_CS_AS LIKE '" + uuid + "'");
            if (dr.HasRows) {
                while (dr.Read()) {
                    contact = new Contacts {
                        Uuid = dr[0].ToString(),
                        Identifier = dr[1].ToString(),
                        Notes = dr[2].ToString(),
                        ExposedOn = Convert.ToDateTime(dr[3]),
                        AddedOn = Convert.ToDateTime(dr[4]),
                        NextVisit = Convert.ToDateTime(dr[5]),
                        Status = dr[6].ToString(),
                        Location = dr[7].ToString(),
                        Relation = dr[8].ToString(),
                        Proximity = dr[9].ToString(),
                        DiseaseAfter = dr[10].ToString(),
                        PrevouslyTreated = dr[11].ToString(),
                    };

                    contact.Person = new Person {
                        Uuid = dr[12].ToString(),
                        Name = dr[13].ToString(),
                        Gender = dr[14].ToString().FirstCharToUpper(),
                        DateOfBirth = Convert.ToDateTime(dr[15])
                    };

                    contact.Index = new PatientProgram {
                        TbmuNumber = dr[16].ToString(),
                        DateEnrolled = Convert.ToDateTime(dr[17]),
                        Patient = new Patient {
                            Uuid = dr[18].ToString(),
                            Dead = Convert.ToInt32(dr[19]),
                            Person = new Person {
                                Uuid = dr[20].ToString(),
                                Name = dr[21].ToString(),
                                Gender = dr[22].ToString().FirstCharToUpper(),
                                DateOfBirth = Convert.ToDateTime(dr[23])
                            }
                        }
                    };

                    if (!string.IsNullOrEmpty(dr[24].ToString()))
                        contact.Index.Patient.DiedOn = Convert.ToDateTime(dr[24]);
                }
            }

            return contact;
        }

        public List<Contacts> GetContacts(string p = "") {
            List<Contacts> contacts = new List<Contacts>();
            string query = "";

            if (!string.IsNullOrEmpty(p))
                query = "WHERE pt_uuid COLLATE SQL_Latin1_General_CP1_CS_AS LIKE '" + p + "'";


            SqlServerConnection conn = new SqlServerConnection(settings);
            SqlDataReader dr = conn.SqlServerConnect("SELECT ct_uuid, ct_identifier, ct_notes, ct_exposed_from, ct_added_on, ct_next_screening, cs.cpt_name [status], cl.cpt_name [location], cr.cpt_name [relation], cp.cpt_name [proximity], cd.cpt_name[disease_after], ct.cpt_name[previously_treated], p.ps_uuid, p.ps_name, p.ps_gender, p.ps_dob, pp_tbmu, pp_enrolled_on, pt_uuid, pt_dead, ps.ps_uuid, ps.ps_name, ps.ps_gender, ps.ps_dob, pt_death_date FROM Contacts INNER JOIN Person p ON ct_person=p.ps_idnt INNER JOIN PatientProgram ON ct_index=pp_idnt INNER JOIN Patient ON pp_patient=pt_idnt INNER JOIN Person ps ON pt_person=ps.ps_idnt INNER JOIN Concept cs ON ct_status= cs.cpt_id INNER JOIN Concept cl ON ct_location= cl.cpt_id INNER JOIN Concept cr ON ct_relationship= cr.cpt_id INNER JOIN Concept cp ON ct_proximity=cp.cpt_id INNER JOIN Concept cd ON ct_desease_after=cd.cpt_id INNER JOIN Concept ct ON ct_prev_treated=ct.cpt_id " + query + " ORDER BY ps.ps_name, ps.ps_uuid, ct_identifier");
            if (dr.HasRows) {
                while (dr.Read()) {
                    Contacts contact = new Contacts {
                        Uuid = dr[0].ToString(),
                        Identifier = dr[1].ToString(),
                        Notes = dr[2].ToString(),
                        ExposedOn = Convert.ToDateTime(dr[3]),
                        AddedOn = Convert.ToDateTime(dr[4]),
                        NextVisit = Convert.ToDateTime(dr[5]),
                        Status = dr[6].ToString(),
                        Location = dr[7].ToString(),
                        Relation = dr[8].ToString(),
                        Proximity = dr[9].ToString(),
                        DiseaseAfter = dr[10].ToString(),
                        PrevouslyTreated = dr[11].ToString(),
                    };

                    contact.Person = new Person {
                        Uuid = dr[12].ToString(),
                        Name = dr[13].ToString(),
                        Gender = dr[14].ToString().FirstCharToUpper(),
                        DateOfBirth = Convert.ToDateTime(dr[15])
                    };

                    contact.Index = new PatientProgram {
                        TbmuNumber = dr[16].ToString(),
                        DateEnrolled = Convert.ToDateTime(dr[17]),
                        Patient = new Patient {
                            Uuid = dr[18].ToString(),
                            Dead = Convert.ToInt32(dr[19]),
                            Person = new Person {
                                Uuid = dr[20].ToString(),
                                Name = dr[21].ToString(),
                                Gender = dr[22].ToString().FirstCharToUpper(),
                                DateOfBirth = Convert.ToDateTime(dr[23])
                            }
                        }
                    };

                    if (!string.IsNullOrEmpty(dr[24].ToString()))
                        contact.Index.Patient.DiedOn = Convert.ToDateTime(dr[24]);

                    contacts.Add(contact);
                }
            }

            return contacts;
        }
    }
}