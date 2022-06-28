using System.Threading.Tasks;
using PostMediumGitHubAction.Domain;

namespace PostMediumGitHubAction.Services;

public interface IMediumService
{
    Task<MediumCreatedPost> SubmitNewContentAsync();

    /// <summary>
    ///     Retrieves current authenticated user
    /// </summary>
    /// <returns>Medium User</returns>
    Task<User> GetCurrentMediumUserAsync();

    /// <summary>
    ///     Retrieve matching publication
    /// </summary>
    /// <param name="mediumUserId">Id of the Medium User</param>
    /// <param name="publicationToLookFor">Name of the Publication to look for</param>
    /// <param name="publicationId">Optional Id of the publication</param>
    /// <returns>Medium Publication</returns>
    Task<Publication> FindMatchingPublicationAsync(string mediumUserId, string publicationToLookFor,
        string publicationId);

    /// <summary>
    ///     Create a new post under a publication
    /// </summary>
    /// <param name="publicationId">The id of the publication</param>
    /// <returns>Medium Created Post</returns>
    Task<MediumCreatedPost> CreateNewPostUnderPublicationAsync(string publicationId);

    /// <summary>
    ///     Create a new post without a publication.
    /// </summary>
    /// <param name="authorId">The id of the author</param>
    /// <returns>Medium Created Post</returns>
    Task<MediumCreatedPost> CreateNewPostWithoutPublicationAsync(string authorId);

    /// <summary>
    ///     Create a new post for either a publication or author
    /// </summary>
    /// <param name="requestUri">The uri of the endpoint</param>
    /// <returns>Medium Created Post</returns>
    Task<MediumCreatedPost> CreateNewPostAsync(string requestUri);

    /// <summary>
    ///     Sets the output variables in the GitHub workflow
    ///     See:
    ///     https://docs.github.com/en/actions/learn-github-actions/workflow-commands-for-github-actions#setting-an-output-parameter
    /// </summary>
    /// <param name="post">Newly created post that is used to set output variables</param>
    /// <returns></returns>
    void SetWorkflowOutputs(MediumCreatedPost post);
}