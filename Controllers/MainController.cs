using System;
using System.Collections.Generic;
using EtbSomalia.Models;
using EtbSomalia.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtbSomalia.Controllers
{
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private ICoreService iservice;

        public UsersController(ICoreService service) { 
            iservice = service;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        [Route("api/authenticate")]
        public IActionResult Authenticate([FromBody]User param) {
            var user = iservice.Authenticate(param.Username, param.Password);
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });
            return Ok(user);
        }

        [HttpGet]
        [Route("api/contacts")]
        public IActionResult GetContacts(string p = "") {
            var contacts = iservice.GetContacts(p);
            return Ok(contacts);
        }

        [HttpGet]
        [Route("api/contacts/{uuid}")]
        public IActionResult GetContactByUuid(string uuid) {
            var contacts = iservice.GetContacts(null, uuid);
            if (contacts.Count.Equals(0))
                return BadRequest("Invalid Contact");
            return Ok(contacts[0]);
        }

        [HttpGet]
        [Route("api/contacts/examinations/{uuid}")]
        public IActionResult GetContactExaminations(string uuid) {
            var exams = iservice.GetContactsExaminations(uuid);
            if (exams.Count.Equals(0))
                return BadRequest("Invalid Contact");
            return Ok(exams[0]);
        }

        [HttpGet]
        [Route("api/contacts/examinations")]
        public IActionResult GetContactsExaminations(string p = "") {
            var exams = iservice.GetContactsExaminations(null, p);
            return Ok(exams);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("api/status")]
        public IActionResult TestAPI() {
            return Ok("Ok");
        }
    }
}