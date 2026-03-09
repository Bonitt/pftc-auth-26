using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pftc_auth.DataAccess;
using pftc_auth.Models;

namespace pftc_auth.Controllers
{
    public class SocialController : Controller
    {
        private FirestoreRepository _repo;
        private ILogger<SocialController> _logger;

        public SocialController(ILogger<SocialController> logger, FirestoreRepository repo)
        {
            _repo = repo;
            _logger = logger;
        }


        //GET
        [Authorize]
        public IActionResult Index()
        {
            return View(_repo.GetPosts().Result);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost(SocialMediaPost p)
        {


            p.PostId = Guid.NewGuid().ToString();
            p.PostAuthor = User.Identity.Name;
            p.PostDate = DateTimeOffset.UtcNow;
            await _repo.CreatePost(p);
            return RedirectToAction("Index", "Social");

        }

        //Delete
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeletePost(string postId)
        {
            try
            {
                await _repo.DeletePost(postId);
                return RedirectToAction("Index", "Social");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"Failed to delete post with id {postId}");
                return NotFound();
            }
        }
        //Update
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdatePost(string postId, string postContent)
        {
            try
            {
                await _repo.UpdatePost(postId, postContent);
                return RedirectToAction("Index", "Social");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"Failed to update post with id {postId}");
                return NotFound();
            }
        }
    }
}
