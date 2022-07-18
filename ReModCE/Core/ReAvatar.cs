using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VRC.Core;

namespace Serpent.Core
{
    internal class AvatarGet
    {
        public List<ReAvatar> records { get; set; }
    }

    [Serializable]
    internal class ReAvatar
    {
        public string AvatarID { get; set; }
        public string AvatarName { get; set; }
        public string AuthorID { get; set; }
        public string AuthorName { get; set; }
        public string AvatarDescription { get; set; }
        public string PCAssetURL { get; set; }
        public string ImageURL { get; set; }
        public string ThumbnailURL { get; set; }
        public string UserId { get; set; }
        public string Pin { get; set; }
        public string Category { get; set; }
        public string Quest { get; set; }

        public ReAvatar()
        {
        }



        public ReAvatar(ApiAvatar apiAvatar)
        {
            AvatarID = apiAvatar.id;
            AvatarName = apiAvatar.name;
            AuthorID = apiAvatar.authorId;
            AuthorName = apiAvatar.authorName;
            AvatarDescription = apiAvatar.description;
            PCAssetURL = apiAvatar.assetUrl;
            ImageURL = "None";
            ThumbnailURL = apiAvatar.thumbnailImageUrl;
            Category = "0";
        }

        public ApiAvatar AsApiAvatar()
        {
            ApiModel.SupportedPlatforms supported = ApiModel.SupportedPlatforms.StandaloneWindows;

            if (Quest == "true")
            {
                supported = ApiModel.SupportedPlatforms.All;
            }

            return new ApiAvatar
            {
                id = AvatarID,
                name = AvatarName,
                authorId = AuthorID,
                authorName = AuthorName,
                description = AvatarDescription,
                assetUrl = PCAssetURL,
                thumbnailImageUrl = string.IsNullOrEmpty(ThumbnailURL) ? (string.IsNullOrEmpty(ImageURL) ? "https://assets.vrchat.com/system/defaultAvatar.png" : ImageURL) : ThumbnailURL,
                releaseStatus = "public",
                unityVersion = "2019.4.31f1",
                version = 1,
                apiVersion = 1,
                Endpoint = "avatars",
                Populated = false,
                assetVersion = new AssetVersion("2019.4.31f1", 0),
                tags = new Il2CppSystem.Collections.Generic.List<string>(0),
                supportedPlatforms = supported,
            };
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
