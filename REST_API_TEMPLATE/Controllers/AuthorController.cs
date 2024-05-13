using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using REST_API_TEMPLATE.Models;
using REST_API_TEMPLATE.Services;
using REST_API_TEMPLATE.DTO;
using Microsoft.AspNetCore.Authorization;

namespace REST_API_TEMPLATE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorController : ControllerBase
    {
        private readonly ILibraryService _libraryService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenRepository _tokenRepository;

        public AuthorController(UserManager<IdentityUser> userManager, ILibraryService libraryService,ITokenRepository tokenRepository)
        {
            _libraryService = libraryService;
            _userManager = userManager;
            _tokenRepository = tokenRepository;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO registerRequestDTO)
        {
            var identityUser = new IdentityUser
            {
                UserName = registerRequestDTO.Username,
                Email = registerRequestDTO.Username
            };
            var identityResult = await _userManager.CreateAsync(identityUser, registerRequestDTO.Password);
            if (identityResult.Succeeded)
            {
                //add roles to this user
                if (registerRequestDTO.Roles != null && registerRequestDTO.Roles.Any())
                {
                    identityResult = await _userManager.AddToRolesAsync(identityUser, registerRequestDTO.Roles);
                }
                if (identityResult.Succeeded)
                {
                    return Ok("Register Successful! Let login!");
                }
            }
            return BadRequest("Something wrong!");
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginRequestDTO.Username);
            if (user != null)
            {
                var checkPasswordResult = await
                _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
                if (checkPasswordResult)
                { //get roles for this user
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles != null)
                    { //create token
                        var jwtToken = _tokenRepository.CreateJWTToken(user, roles.ToList());
                        var response = new LoginResponseDTO
                        {
                            JwtToken = jwtToken
                        };
                        return Ok(response);
                    }
                }
            }
            return BadRequest("Username or password incorrect");

        }

        [HttpGet("get-all-author")]
        [Authorize(Roles ="Read,Write")]
        public async Task<IActionResult> GetAuthors()
        {
            var authors = await _libraryService.GetAuthorsAsync();

            if (authors == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No authors in database");
            }

            return StatusCode(StatusCodes.Status200OK, authors);
        }

        [HttpGet("get-author-by-id/{id}")]
        [Authorize(Roles = "Read,Write")]
        public async Task<IActionResult> GetAuthor(Guid id, bool includeBooks = false)
        {
            Author author = await _libraryService.GetAuthorAsync(id, includeBooks);

            if (author == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, $"No Author found for id: {id}");
            }

            return StatusCode(StatusCodes.Status200OK, author);
        }


        [Authorize(Roles = "Editor")]
        [HttpPost]
        public async Task<ActionResult<Author>> AddAuthor(Author author)
        {
            var dbAuthor = await _libraryService.AddAuthorAsync(author);

            if (dbAuthor == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{author.Name} could not be added.");
            }

            return CreatedAtAction("GetAuthor", new { id = author.Id }, author);
        }

        [Authorize(Roles = "Editor")]
        [HttpPut("id")]
        public async Task<IActionResult> UpdateAuthor(Guid id, Author author)
        {
            if (id != author.Id)
            {
                return BadRequest();
            }

            Author dbAuthor = await _libraryService.UpdateAuthorAsync(author);

            if (dbAuthor == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{author.Name} could not be updated");
            }

            return NoContent();
        }

        [Authorize(Roles = "Editor")]
        [HttpDelete("id")]
        public async Task<IActionResult> DeleteAuthor(Guid id)
        {
            var author = await _libraryService.GetAuthorAsync(id, false);
            (bool status, string message) = await _libraryService.DeleteAuthorAsync(author);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, author);
        }


    }
}