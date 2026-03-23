namespace Application.Common.Responses;

public static class FollowMessages
{
    public const string NotAuthenticated = "User is not authenticated.";

    public const string FollowingIdRequired = "Following user id cannot be empty.";

    public const string CannotFollowSelf = "You cannot follow yourself.";
    public const string CannotUnfollowSelf = "You cannot unfollow yourself.";

    public const string UserNotFound = "The user does not exist.";

    public const string AlreadyFollowing = "You are already following this user.";
    public const string NotFollowing = "You are not following this user.";

    public const string FollowSuccess = "User followed successfully.";
    public const string UnfollowSuccess = "User unfollowed successfully.";
    public const string FollowStatsRetrieved = "Follow statistics retrieved successfully.";
    public const string FollowersRetrieved = "Followers retrieved successfully.";
    public const string FollowingsRetrieved = "Followings retrieved successfully.";
    public const string MutualFollowingsRetrieved = "Mutual followings retrieved successfully.";
    public const string FollowRelationshipRetrieved = "Follow relationship retrieved successfully.";
    public const string FollowInsightsRetrieved = "Follow insights retrieved successfully.";

}