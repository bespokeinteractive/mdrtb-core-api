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

        List<Contacts> GetContacts(string p = "", string uuid = "");
        List<ContactsExamination> GetContactsExaminations(string ct = "", string p = "");
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

        public List<Contacts> GetContacts(string p = "", string uuid = "") {
            List<Contacts> contacts = new List<Contacts>();
            string query = "WHERE ct_idnt>0";

            if (!string.IsNullOrEmpty(p))
                query = " AND pt_uuid COLLATE SQL_Latin1_General_CP1_CS_AS LIKE '" + p + "'";
            if (!string.IsNullOrEmpty(uuid))
                query = " AND ct_uuid COLLATE SQL_Latin1_General_CP1_CS_AS LIKE '" + uuid + "'";

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
                        Person = new Person {
                            Uuid = dr[12].ToString(),
                            Name = dr[13].ToString(),
                            Gender = dr[14].ToString().FirstCharToUpper(),
                            DateOfBirth = Convert.ToDateTime(dr[15])
                        },
                        Index = new PatientProgram {
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
                        }
                    };

                    if (!string.IsNullOrEmpty(dr[24].ToString()))
                        contact.Index.Patient.DiedOn = Convert.ToDateTime(dr[24]);

                    contacts.Add(contact);
                }
            }

            return contacts;
        }

        public List<ContactsExamination> GetContactsExaminations(string ct = "", string p = "") {
            List<ContactsExamination> examinations = new List<ContactsExamination>();

            string query = "WHERE ce_idnt>0";

            if (!string.IsNullOrEmpty(ct))
                query += " AND ct_uuid COLLATE SQL_Latin1_General_CP1_CS_AS LIKE '" + ct + "'";
            if (!string.IsNullOrEmpty(p))
                query += " AND pt_uuid COLLATE SQL_Latin1_General_CP1_CS_AS LIKE '" + p + "'";

            SqlServerConnection conn = new SqlServerConnection(settings);
            SqlDataReader dr = conn.SqlServerConnect("SELECT ce_uuid, ce_cough, ce_fever, ce_weight_loss, ce_night_sweats, sp.cpt_name, lt.cpt_name, gx.cpt_name, xr.cpt_name, ISNULL(NULLIF(ce_preventive_regimen,'No'),'N/A')pt, ce_next_screening, ce_added_on, ct_uuid, ct_identifier, ct_notes, ct_exposed_from, ct_added_on, ct_next_screening, cs.cpt_name [status], cl.cpt_name [location], cr.cpt_name [relation], cp.cpt_name [proximity], cd.cpt_name[disease_after], ct.cpt_name[previously_treated], p.ps_uuid, p.ps_name, p.ps_gender, p.ps_dob, pp_tbmu, pp_enrolled_on, pt_uuid, pt_dead, ps.ps_uuid, ps.ps_name, ps.ps_gender, ps.ps_dob, pt_death_date FROM ContactsExaminations INNER JOIN Concept sp ON sp.cpt_id=ce_sputum_smear INNER JOIN Concept lt ON lt.cpt_id=ce_ltbi INNER JOIN Concept gx ON gx.cpt_id=ce_genxpert INNER JOIN Concept xr ON xr.cpt_id=ce_xray_exam INNER JOIN Contacts ON ce_contact=ct_idnt INNER JOIN Person p ON ct_person=p.ps_idnt INNER JOIN PatientProgram ON ct_index=pp_idnt INNER JOIN Patient ON pp_patient=pt_idnt INNER JOIN Person ps ON pt_person=ps.ps_idnt INNER JOIN Concept cs ON ct_status= cs.cpt_id INNER JOIN Concept cl ON ct_location= cl.cpt_id INNER JOIN Concept cr ON ct_relationship= cr.cpt_id INNER JOIN Concept cp ON ct_proximity=cp.cpt_id INNER JOIN Concept cd ON ct_desease_after=cd.cpt_id INNER JOIN Concept ct ON ct_prev_treated=ct.cpt_id " + query + " ORDER BY ps.ps_name, ps.ps_uuid, ct_identifier");
            if (dr.HasRows) {
                while (dr.Read()) {
                    examinations.Add(new ContactsExamination {
                        Uuid = dr[0].ToString(),
                        Cough = Convert.ToBoolean(dr[1]),
                        Fever = Convert.ToBoolean(dr[2]),
                        WeightLoss = Convert.ToBoolean(dr[3]),
                        NightSweat = Convert.ToBoolean(dr[4]),

                        SputumSmear = dr[5].ToString(),
                        LTBI = dr[6].ToString(),
                        GeneXpert = dr[7].ToString(),
                        XrayExam = dr[8].ToString(),
                        PreventiveTherapy = dr[9].ToString(),

                        NextScreening = Convert.ToDateTime(dr[10]),
                        AddedOn = Convert.ToDateTime(dr[11]),

                        Contact = new Contacts {
                            Uuid = dr[12].ToString(),
                            Identifier = dr[13].ToString(),
                            Notes = dr[14].ToString(),
                            ExposedOn = Convert.ToDateTime(dr[15]),
                            AddedOn = Convert.ToDateTime(dr[16]),
                            NextVisit = Convert.ToDateTime(dr[17]),
                            Status = dr[18].ToString(),
                            Location = dr[19].ToString(),
                            Relation = dr[20].ToString(),
                            Proximity = dr[21].ToString(),
                            DiseaseAfter = dr[22].ToString(),
                            PrevouslyTreated = dr[23].ToString(),
                            Person = new Person {
                                Uuid = dr[24].ToString(),
                                Name = dr[25].ToString(),
                                Gender = dr[26].ToString().FirstCharToUpper(),
                                DateOfBirth = Convert.ToDateTime(dr[27])
                            },
                            Index = new PatientProgram {
                                TbmuNumber = dr[28].ToString(),
                                DateEnrolled = Convert.ToDateTime(dr[29]),
                                Patient = new Patient {
                                    Uuid = dr[30].ToString(),
                                    Dead = Convert.ToInt32(dr[31]),
                                    Person = new Person {
                                        Uuid = dr[32].ToString(),
                                        Name = dr[33].ToString(),
                                        Gender = dr[34].ToString().FirstCharToUpper(),
                                        DateOfBirth = Convert.ToDateTime(dr[35])
                                    }
                                }
                            }
                        }
                    });
                }
            }

            return examinations;
        }
    }
}