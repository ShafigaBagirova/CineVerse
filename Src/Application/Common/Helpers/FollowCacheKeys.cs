namespace Application.Common.Helpers;

public static class FollowCacheKeys
{
    public static string Followers(string userId, int page, int pageSize)
        => $"follows:followers:{userId}:page:{page}:size:{pageSize}";

    public static string Followings(string userId, int page, int pageSize)
        => $"follows:followings:{userId}:page:{page}:size:{pageSize}";

    public static string FollowersPattern(string userId)
        => $"follows:followers:{userId}:";

    public static string FollowingsPattern(string userId)
        => $"follows:followings:{userId}:";
    public static string MutualFollowings(string currentUserId, string targetUserId, int page, int pageSize)
    => $"follows:mutual:{currentUserId}:{targetUserId}:page:{page}:size:{pageSize}";

    public static string MutualFollowingsPattern(string currentUserId, string targetUserId)
        => $"follows:mutual:{currentUserId}:{targetUserId}:";
}