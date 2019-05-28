using System;
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
        public IActionResult Authenticate([FromBody]User userParam) {
            var user = iservice.Authenticate(userParam.Username, userParam.Password);

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
        public IActionResult GetContactByUuid(string uuid)
        {
            var contacts = iservice.GetContact(uuid);
            return Ok(contacts);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("api/status")]
        public IActionResult TestAPI() {
            return Ok("Ok");
        }
    }
}