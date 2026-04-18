export interface Movie {
  id: number
  title: string
  year: number
  rating: number
  runtime: string
  genres: string[]
  poster: string
  backdrop: string
  description: string
  director: string
  cast: string[]
  streaming: string[]
  language: string
}

export interface Showtime {
  cinema: string
  times: string[]
  price: number
}

export interface Review {
  user: string
  avatar: string
  rating: number
  text: string
  date: string
  id?: string
  userId?: number
  likes?: number
  liked?: boolean
}

export interface UserProfile {
  id: number
  name: string
  avatar: string
  bio: string
  followers: number
  following: number
  isFollowing: boolean
  vipStatus: boolean
  joinDate: string
  movieCount: number
}

export interface MovieAnalysis {
  averageRating: number
  ratingDistribution: { [key: number]: number }
  topGenre: string
  genreBreakdown: { [key: string]: number }
  topCast: string[]
  streamingAvailability: string[]
}

export interface Notification {
  id: string
  type: 'follow' | 'review' | 'booking' | 'recommendation'
  title: string
  message: string
  timestamp: string
  read: boolean
  actionLink?: string
}

export interface FoodItem {
  id: number
  name: string
  price: number
  image: string
  category: string
}

export interface Ticket {
  id: string
  movie: string
  cinema: string
  date: string
  time: string
  seats: string[]
  poster: string
}

export const movies: Movie[] = [
  {
    id: 1,
    title: "Echoes of Tomorrow",
    year: 2026,
    rating: 8.4,
    runtime: "2h 18m",
    genres: ["Sci-Fi", "Thriller"],
    poster: "/images/movie-1.jpg",
    backdrop: "/images/movie-1.jpg",
    description: "In a world where memories can be traded like currency, a rogue memory dealer discovers a fragment that could unravel the fabric of reality itself. As powerful forces close in, she must decide between erasing the truth or letting it destroy everything she knows.",
    director: "Sofia Volkov",
    cast: ["Aria Chen", "Marcus Webb", "Elena Petrova", "James Nakamura"],
    streaming: ["Netflix", "Prime"],
    language: "English"
  },
  {
    id: 2,
    title: "Golden Horizons",
    year: 2025,
    rating: 7.9,
    runtime: "1h 54m",
    genres: ["Romance", "Drama"],
    poster: "/images/movie-2.jpg",
    backdrop: "/images/movie-2.jpg",
    description: "Two strangers from different worlds find their lives intertwined during a summer in the Mediterranean, discovering that love can bridge even the widest cultural divides.",
    director: "Luca Moretti",
    cast: ["Isabella Rossi", "Ahmed Hassan", "Claire Dubois"],
    streaming: ["Prime"],
    language: "Spanish"
  },
  {
    id: 3,
    title: "The Vanishing Act",
    year: 2026,
    rating: 8.7,
    runtime: "2h 05m",
    genres: ["Thriller", "Mystery"],
    poster: "/images/movie-3.jpg",
    backdrop: "/images/movie-3.jpg",
    description: "When a renowned illusionist disappears during his final performance, his daughter must unravel a web of secrets that stretches back decades to find him before it's too late.",
    director: "Nora Blackwell",
    cast: ["Zara Knight", "David Park", "Helen Moore"],
    streaming: ["Netflix"],
    language: "English"
  },
  {
    id: 4,
    title: "Starfall Kingdom",
    year: 2026,
    rating: 8.1,
    runtime: "1h 48m",
    genres: ["Animation", "Adventure"],
    poster: "/images/movie-4.jpg",
    backdrop: "/images/movie-4.jpg",
    description: "A young girl discovers she holds the key to restoring magic to a once-enchanted kingdom floating among the stars, embarking on an epic journey with unlikely companions.",
    director: "Yuki Tanaka",
    cast: ["Lily Zhang (voice)", "Tom Harris (voice)", "Maya Patel (voice)"],
    streaming: ["Disney+"],
    language: "Japanese"
  },
  {
    id: 5,
    title: "Wrath of Titans",
    year: 2025,
    rating: 7.5,
    runtime: "2h 22m",
    genres: ["Action", "Adventure"],
    poster: "/images/movie-5.jpg",
    backdrop: "/images/movie-5.jpg",
    description: "An ancient warrior awakens to find his homeland under siege by mythological beasts. Armed with a legendary weapon, he must unite the scattered tribes to face an enemy that threatens all of existence.",
    director: "Kenji Oduya",
    cast: ["Ravi Kapoor", "Sarah Lindqvist", "Omar Fadel"],
    streaming: ["Prime", "HBO Max"],
    language: "Korean"
  },
  {
    id: 6,
    title: "Hollow Manor",
    year: 2026,
    rating: 7.8,
    runtime: "1h 58m",
    genres: ["Horror", "Mystery"],
    poster: "/images/movie-6.jpg",
    backdrop: "/images/movie-6.jpg",
    description: "A family inherits an old manor with a dark past. As they settle in, they discover the house holds memories of its own, and not all of them want to stay buried.",
    director: "Camille Noir",
    cast: ["Jessica Albright", "Marcus Chen", "Oliver Stone"],
    streaming: ["Netflix", "Hulu"],
    language: "English"
  },
]

export const showtimes: Record<number, Showtime[]> = {
  1: [
    { cinema: "CinemaPlus 28 Mall", times: ["14:00", "17:30", "20:00", "22:30"], price: 12 },
    { cinema: "Park Cinema Flame Towers", times: ["15:00", "18:00", "21:00"], price: 15 },
    { cinema: "CinemaPlus Ganjlik", times: ["13:30", "16:00", "19:00", "21:30"], price: 10 },
  ],
  2: [
    { cinema: "CinemaPlus 28 Mall", times: ["13:00", "16:30", "19:00"], price: 12 },
    { cinema: "Park Cinema Flame Towers", times: ["14:30", "18:00", "21:00"], price: 15 },
  ],
  3: [
    { cinema: "CinemaPlus 28 Mall", times: ["15:00", "18:30", "21:00"], price: 12 },
    { cinema: "Park Cinema Flame Towers", times: ["16:00", "19:30", "22:00"], price: 15 },
    { cinema: "CinemaPlus Ganjlik", times: ["14:00", "17:30", "20:30"], price: 10 },
  ],
  4: [
    { cinema: "CinemaPlus 28 Mall", times: ["11:00", "13:30", "16:00", "18:30"], price: 12 },
    { cinema: "Park Cinema Flame Towers", times: ["12:00", "15:00", "17:30"], price: 15 },
  ],
  5: [
    { cinema: "CinemaPlus 28 Mall", times: ["14:30", "17:00", "20:00", "22:30"], price: 12 },
    { cinema: "CinemaPlus Ganjlik", times: ["15:00", "18:30", "21:30"], price: 10 },
  ],
  6: [
    { cinema: "Park Cinema Flame Towers", times: ["19:00", "21:30", "23:30"], price: 15 },
    { cinema: "CinemaPlus Ganjlik", times: ["18:00", "20:30", "23:00"], price: 10 },
  ],
}

export const reviews: Record<number, Review[]> = {
  1: [
    { user: "FilmBuff92", avatar: "FB", rating: 9, text: "A mind-bending masterpiece that redefines the sci-fi genre. The memory-trading concept is brilliantly executed.", date: "Feb 15, 2026" },
    { user: "CinemaLover", avatar: "CL", rating: 8, text: "Visually stunning with a thought-provoking narrative. Aria Chen delivers a career-best performance.", date: "Feb 12, 2026" },
    { user: "MovieCritic_AZ", avatar: "MC", rating: 8, text: "The pacing is perfect and the twist at the end genuinely surprised me. Must watch in IMAX.", date: "Feb 10, 2026" },
  ],
  3: [
    { user: "ThrillerFan", avatar: "TF", rating: 9, text: "Keeps you on the edge of your seat from start to finish. Nora Blackwell has outdone herself.", date: "Feb 14, 2026" },
    { user: "AZMovieNerd", avatar: "AM", rating: 8, text: "The mystery unfolds beautifully. Great performances all around.", date: "Feb 11, 2026" },
  ],
}

export const userTickets: Ticket[] = [
  {
    id: "TK-001",
    movie: "Echoes of Tomorrow",
    cinema: "CinemaPlus 28 Mall",
    date: "Feb 22, 2026",
    time: "20:00",
    seats: ["F7", "F8"],
    poster: "/images/movie-1.jpg"
  },
  {
    id: "TK-002",
    movie: "The Vanishing Act",
    cinema: "Park Cinema Flame Towers",
    date: "Feb 25, 2026",
    time: "19:30",
    seats: ["D12"],
    poster: "/images/movie-3.jpg"
  },
]

export const foodItems: FoodItem[] = [
  { id: 1, name: "Large Popcorn", price: 6, image: "popcorn", category: "snacks" },
  { id: 2, name: "Nachos & Cheese", price: 8, image: "nachos", category: "snacks" },
  { id: 3, name: "Coca-Cola (L)", price: 4, image: "drink", category: "drinks" },
  { id: 4, name: "Combo Deal", price: 14, image: "combo", category: "combo" },
  { id: 5, name: "Candy Mix", price: 7, image: "candy", category: "snacks" },
  { id: 6, name: "Water Bottle", price: 3, image: "water", category: "drinks" },
]

export const userWatchedMovies = [1, 3, 5]

export const userWatchlist = [2, 4, 6]

export const suggestedUsers: UserProfile[] = [
  {
    id: 1,
    name: "Alex Chen",
    avatar: "AC",
    bio: "Film critic and enthusiast | Baku, Azerbaijan",
    followers: 2540,
    following: 180,
    isFollowing: false,
    vipStatus: true,
    joinDate: "Jan 2024",
    movieCount: 287,
  },
  {
    id: 2,
    name: "Sarah Wilson",
    avatar: "SW",
    bio: "Movie lover | Always watching something new",
    followers: 1203,
    following: 342,
    isFollowing: false,
    vipStatus: false,
    joinDate: "Mar 2024",
    movieCount: 156,
  },
  {
    id: 3,
    name: "Ravi Patel",
    avatar: "RP",
    bio: "Sci-Fi enthusiast | Documentary junkie",
    followers: 3420,
    following: 215,
    isFollowing: false,
    vipStatus: true,
    joinDate: "Nov 2023",
    movieCount: 412,
  },
]

export const notifications: Notification[] = [
  {
    id: "1",
    type: "follow",
    title: "New Follower",
    message: "Alex Chen started following you",
    timestamp: "2 hours ago",
    read: false,
    actionLink: "/profile/1"
  },
  {
    id: "2",
    type: "review",
    title: "New Review",
    message: "Sarah Wilson reviewed 'The Vanishing Act': Excellent thriller!",
    timestamp: "4 hours ago",
    read: false,
    actionLink: "/movies/3"
  },
  {
    id: "3",
    type: "booking",
    title: "Ticket Reminder",
    message: "Your ticket for 'Echoes of Tomorrow' is ready for pickup",
    timestamp: "1 day ago",
    read: true,
    actionLink: "/tickets"
  },
  {
    id: "4",
    type: "recommendation",
    title: "Recommended for You",
    message: "Based on your taste, you might like 'Wrath of Titans'",
    timestamp: "2 days ago",
    read: true,
    actionLink: "/movies/5"
  },
]

export const movieAnalysis: Record<number, MovieAnalysis> = {
  1: {
    averageRating: 8.4,
    ratingDistribution: { 5: 45, 4: 30, 3: 15, 2: 7, 1: 3 },
    topGenre: "Sci-Fi",
    genreBreakdown: { "Sci-Fi": 100, "Thriller": 100 },
    topCast: ["Aria Chen", "Marcus Webb", "Elena Petrova"],
    streamingAvailability: ["Netflix", "Prime Video"],
  },
  2: {
    averageRating: 7.9,
    ratingDistribution: { 5: 38, 4: 35, 3: 20, 2: 5, 1: 2 },
    topGenre: "Romance",
    genreBreakdown: { "Romance": 100, "Drama": 100 },
    topCast: ["Isabella Rossi", "Ahmed Hassan", "Claire Dubois"],
    streamingAvailability: ["Prime Video"],
  },
  3: {
    averageRating: 8.7,
    ratingDistribution: { 5: 52, 4: 28, 3: 12, 2: 5, 1: 3 },
    topGenre: "Thriller",
    genreBreakdown: { "Thriller": 100, "Mystery": 100 },
    topCast: ["Zara Knight", "David Park", "Helen Moore"],
    streamingAvailability: ["Netflix"],
  },
}
