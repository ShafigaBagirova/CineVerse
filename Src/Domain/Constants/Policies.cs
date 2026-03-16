namespace Domain.Constants;

public static class Policies
{
    public const string Authenticated = "Authenticated";

    public const string AdminOnly = "AdminOnly";
    public const string ManageMovies = "ManageMovies";

    public const string VipOnly = "VipOnly";

    public const string ManageCinemas = "ManageCinemas";
    public const string ManageScreenings = "ManageScreenings";
    public const string ManageFoods = "ManageFoods";

    public const string PurchaseTicket = "PurchaseTicket";

    public const string ReviewOwnerOrAdmin = "ReviewOwnerOrAdmin";
    public const string RatingOwnerOrAdmin = "RatingOwnerOrAdmin";
    public const string TicketOwnerOrAdmin = "TicketOwnerOrAdmin";
    public const string FoodOrderOwnerOrAdmin = "FoodOrderOwnerOrAdmin";

    public const string ViewFriendTicketActivity = "ViewFriendTicketActivity";
    public const string ViewTasteMatchSuggestions = "ViewTasteMatchSuggestions";
}
