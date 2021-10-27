namespace PostMediumGitHubAction
{
    public class Settings
    {
        // The post's title
        public string Title { get; set; }
        // The post’s tags
        public string[] Tags { get; set; }
        // The canonical URL of the post. 
        // If canonicalUrl was not specified in the creation of the post, 
        // this field will not be present.
        public string CanonicalUrl { get; set; }
        // The publish status of the post.
        // Valid values are: Draft, Public or Unlisted
        // Default is Draft
        public string PublishStatus { get; set; } = "Draft";
        // The license of the post.
        // Valid values are all-rights-reserved, cc-40-by, cc-40-by-sa, cc-40-by-nd, 
        // cc-40-by-nc, cc-40-by-nc-nd, cc-40-by-nc-sa, cc-40-zero, public-domain. 
        //Default all-rights-reserved.
        public string License { get; set; } = "all-rights-reserved";
        // The id of the publication the post is being created under.
        // If you do not know the Id, use PublicationName
        public int PublicationId { get; set; }
        // The name of the publication the post is being created under.
        // Either PublicationName of PublicationId should be set.
        public string PublicationName { get; set; }
        // Whether to notify followers that the user has published.
        public bool NotifyFollowers { get; set; }
        // The format of the "content" field. There are two valid values, "html", and "markdown"
        public string ContentFormat { get; set; }
        // The body of the post, in a valid, semantic, HTML fragment, or Markdown. 
        // Further markups may be supported in the future. 
        // If you want your title to appear on the post page, you must also include it as part of the post content.
        public string Content { get; set; }
        // Path to the file that should be read and used as content.
        // If this is set, this will overrule the Content property.
        public string File { get; set; }
        // The token that should be used to authenticate in the API
        // Token can be retrieved at https://medium.com/me/settings under "Integration Token"
        public string IntegrationToken { get; set; }
    }
}