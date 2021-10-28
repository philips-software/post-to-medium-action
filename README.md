<div id="top"></div>

<div align="center">

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

</div>

<br />
<div align="center">
  <h3 align="center">Post to Medium GitHub Action</h3>

  <p align="center">
    GitHub Action that allows you to automatically create a new Medium post with Markdown or HTML.
    <br>
    <a href="https://github.com/philips-software/post-to-medium-action/issues">Report Bug</a>
    ·
    <a href="https://github.com/philips-software/post-to-medium-action/issues">Request Feature</a>
  </p>
</div>

<p align="right">(<a href="#top">back to top</a>)</p>

## Usage

The easiest way to use this action is to add the following into your workflow file. Additional configuration might be necessary to fit your usecase.

1. Add the following part in your workflow file:

   ```yaml
   execute-action:
    name: Use internal GH Action
    runs-on: ubuntu-latest
    steps:
      - name: Create Medium Post
        uses: philips-software/post-to-medium-action@latest
        with:
           IntegrationToken: "${{ secrets.INTEGRATION_TOKEN }}"
           Content: "Content here"
           ContentFormat: "markdown"
           NotifyFollowers: "false"
           PublicationName: "publication"
           Tags: "test,tag"
           Title: "Test post via GH Actions to Medium"
   ```


## Inputs

| parameter | description | required | default |
| - | - | - | - |
| IntegrationToken | Medium's Integration Token. Token can be retrieved at medium.com, settings section, under 'Integration Token.' | `true` |  |
| Content | Content to add in the post, can be either HTML or Markdown. Use either this parameter, or the file parameter. | `false` |  |
| ContentFormat | The format of the content field. There are two valid values, html, and markdown. | `true` |  |
| File | Absolute path to the file to use as content, can be either HTML or Markdown. Use either this parameter, or the content parameter. | `false` |  |
| PublishStatus | Post's status. Valid values are 'draft', 'public', or 'unlisted'. | `false` | draft |
| NotifyFollowers | Whether to notify followers that the user has published. | `false` | false |
| License | Post's license. Valid values are 'all-rights-reserved', 'cc-40-by', 'cc-40-by-sa', 'cc-40-by-nd', 'cc-40-by-nc', 'cc-40-by-nc-nd', 'cc-40-by-nc-sa', 'cc-40-zero', 'public-domain'. | `false` | all-rights-reserved |
| PublicationName | The name of the publication the post is being created under. Either PublicationName of PublicationId should be set. | `false` |  |
| PublicationId | The id of the publication the post is being created under. If you do not know the Id, use PublicationName. | `false` |  |
| CanonicalUrl | The canonical URL of the post. If canonicalUrl was not specified in the creation of the post, this field will not be present. | `false` |  |
| Tags | The post’s tags. Provide a comma separated string without spaces. | `true` |  |
| Title | The post's title. | `true` |  |


## Outputs

| parameter | description |
| - | - |
| id | ID of the Medium post. |
| authorId | Author ID of the post creator. |
| canonicalUrl | Canonical URL of the post. |
| license | License of the post, can be empty at times. |
| licenseUrl | License url of the post, Medium uses this under the hood. |
| publicationId | Id of the publication which the post is created under. |
| publicationStatus | Publication status of the post. |
| title | Title of the post. |
| tags | Tags of the post, comma separated. |
| url | URL to the Medium post. |

## Contributing

If you have a suggestion that would make this project better, please fork the repository and create a pull request. You can also simply open an issue with the tag "enhancement".

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

Distributed under the MIT License. See [LICENSE](/LICENSE) for more information.

<p align="right">(<a href="#top">back to top</a>)</p>

[contributors-shield]: https://img.shields.io/github/contributors/philips-software/post-to-medium-action.svg?style=for-the-badge
[contributors-url]: https://github.com/philips-software/post-to-medium-action/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/philips-software/post-to-medium-action.svg?style=for-the-badge
[forks-url]: https://github.com/philips-software/post-to-medium-action/network/members
[stars-shield]: https://img.shields.io/github/stars/philips-software/post-to-medium-action.svg?style=for-the-badge
[stars-url]: https://github.com/philips-software/post-to-medium-action/stargazers
[issues-shield]: https://img.shields.io/github/issues/philips-software/post-to-medium-action.svg?style=for-the-badge
[issues-url]: https://github.com/philips-software/post-to-medium-action/issues
[license-shield]: https://img.shields.io/github/license/philips-software/post-to-medium-action.svg?style=for-the-badge
[license-url]: https://github.com/philips-software/post-to-medium-action/blob/main/LICENSE
