using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using pftc_auth.Models;

namespace pftc_auth.DataAccess
{
    public class FirestoreRepository
    {
        private readonly ILogger<FirestoreRepository> _logger;

        private FirestoreDb _db;

        public FirestoreRepository(ILogger<FirestoreRepository> logger, IConfiguration configuration)
        {
            _logger = logger;
            _db = FirestoreDb.Create(configuration["Authentication:Google:ProjectId"]);
        }

        public async Task<SocialMediaPost> GetPostById(string postId)
        {
            Query allPostsQuery = _db.Collection("posts").WhereEqualTo("postId", postId);
            QuerySnapshot querySnapshot = await allPostsQuery.GetSnapshotAsync();

            if (querySnapshot.Documents.Count == 0)
                throw new KeyNotFoundException($"Post with id {postId} not found");

            DocumentSnapshot documentSnapshot = querySnapshot.Documents[0];
            SocialMediaPost post = documentSnapshot.ConvertTo<SocialMediaPost>();

            _logger.LogInformation($"Returning post {postId} by {post.PostAuthor}");
            return post;
        }
        public async Task CreatePost(SocialMediaPost p)
        {
            await _db.Collection("posts").AddAsync(p);
            _logger.LogInformation($"Post Created {p.PostAuthor} created successfully.");
        }

        private async Task<DocumentSnapshot> GetPostsAsQuerySnapshot(string postId)
        {
            List<SocialMediaPost> posts = new List<SocialMediaPost>();

            Query allPostsQuery = _db.Collection("posts").WhereEqualTo("postId", postId);
            QuerySnapshot querySnapshot = await allPostsQuery.GetSnapshotAsync();
            if (querySnapshot.Documents.Count == 0)
                throw new KeyNotFoundException($"Post with id {postId} not found");

            return querySnapshot.Documents[0];

        }

        public async Task<List<SocialMediaPost>> GetPosts()
        {
            List<SocialMediaPost> posts = new List<SocialMediaPost>();
            _logger.LogInformation("Fetching all posts from Firestore.");

            CollectionReference postsRef = _db.Collection("posts");

            QuerySnapshot snapshot = await postsRef.GetSnapshotAsync();

            posts = snapshot.Documents
                .Select(doc => doc.ConvertTo<SocialMediaPost>())
                .ToList();

            _logger.LogInformation($"Successfully retrieved {posts.Count} posts.");
            return posts;
        }


        public async Task UpdatePost(string postId, string postContent)
        {
            if (string.IsNullOrEmpty(postId))
            {
                throw new ArgumentException("Post ID cannot be null or empty.", nameof(postId));

            }

            try
            {
                DocumentSnapshot documentSnapshot = await GetPostsAsQuerySnapshot(postId);

                //Update it
                await documentSnapshot.Reference.UpdateAsync("postContent", postContent);
                _logger.LogInformation($"Post with ID: {postId} updated successfully.");

                //if later on we have multimedia (images, videos) these need to be deleted as well
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                _logger.LogError(ex, $"Error deleting post with ID: {postId}");
                throw;
            }
        }


        public async Task DeletePost(string PostId)
        {
            if (string.IsNullOrEmpty(PostId))
            {
                throw new ArgumentException("Post ID cannot be null or empty.", nameof(PostId));

            }

            try
            {
                Query allPostsQuery = _db.Collection("posts").WhereEqualTo("postId", PostId);
                QuerySnapshot querySnapshot = await allPostsQuery.GetSnapshotAsync();

                if (querySnapshot.Documents.Count == 0)
                    throw new KeyNotFoundException($"Post with id {PostId} not found");

                DocumentSnapshot documentSnapshot = querySnapshot.Documents[0];

                await documentSnapshot.Reference.DeleteAsync();

                //if later on we have multimedia (images, videos) these need to be deleted as well
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                _logger.LogError(ex, $"Error deleting post with ID: {PostId}");
                throw;
            }
        }
    }
}
