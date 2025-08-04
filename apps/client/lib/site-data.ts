export const siteData = {
  // Brand Information
  brand: {
    name: "TravelEase",
    tagline: "Your Journey, Our Expertise",
    description:
      "Discover the world with confidence. Book hotels, flights, and experiences with ease.",
    logo: {
      text: "TravelEase",
      alt: "TravelEase Logo",
    },
  },

  // Navigation
  navigation: {
    main: [
      { name: "Home", href: "/" },
      { name: "Hotels", href: "/hotels" },
      { name: "Deals", href: "/deals" },
      { name: "About", href: "/about" },
    ],
    user: [
      { name: "Dashboard", href: "/dashboard" },
      { name: "My Bookings", href: "/dashboard/bookings" },
      { name: "Profile", href: "/dashboard/profile" },
      { name: "Settings", href: "/dashboard/settings" },
    ],
  },

  // Homepage Content
  homepage: {
    hero: {
      title: "Discover Your Perfect Getaway",
      subtitle:
        "Explore thousands of destinations and find the best deals on hotels, flights, and experiences worldwide.",
      cta: {
        primary: "Start Exploring",
        secondary: "View Deals",
      },
    },
    features: [
      {
        title: "Best Price Guarantee",
        description:
          "We guarantee the best prices for your bookings. If you find a lower price elsewhere, we'll match it.",
        icon: "price-tag",
      },
      {
        title: "24/7 Support",
        description:
          "Our customer support team is available around the clock to help with your travel needs.",
        icon: "support",
      },
      {
        title: "Secure Booking",
        description:
          "Your personal information and payments are protected with industry-standard security measures.",
        icon: "shield",
      },
      {
        title: "Flexible Cancellation",
        description:
          "Most bookings can be cancelled or modified up to 24 hours before your stay.",
        icon: "calendar",
      },
    ],
    stats: [
      { number: "500+", label: "Destinations" },
      { number: "10K+", label: "Hotels" },
      { number: "1M+", label: "Happy Travelers" },
      { number: "24/7", label: "Support" },
    ],
  },

  // Authentication
  auth: {
    login: {
      title: "Welcome Back",
      subtitle: "Sign in to your account to continue your journey",
      email: "Email Address",
      password: "Password",
      forgotPassword: "Forgot your password?",
      loginButton: "Sign In",
      googleLogin: "Continue with Google",
      noAccount: "Don't have an account?",
      signUp: "Sign up",
      emailPlaceholder: "Enter your email",
    },
    register: {
      title: "Create Your Account",
      subtitle: "Join thousands of travelers and start your journey today",
      firstName: "First Name",
      lastName: "Last Name",
      email: "Email Address",
      password: "Password",
      confirmPassword: "Confirm Password",
      terms: "I agree to the Terms of Service and Privacy Policy",
      registerButton: "Create Account",
      googleRegister: "Sign up with Google",
      haveAccount: "Already have an account?",
      signIn: "Sign in",
    },
  },

  // Footer
  footer: {
    sections: [
      {
        title: "Travel Services",
        links: [
          { title: "Hotel Booking", href: "/hotels" },
          { title: "Flight Booking", href: "/flights" },
          // { title: "Car Rentals", href: "/cars" },
          // { title: "Travel Insurance", href: "/insurance" },
          // { title: "Visa Services", href: "/visa" },
          // { title: "Travel Guides", href: "/guides" },
        ],
      },
      {
        title: "Company",
        links: [
          { title: "About Us", href: "/about" },
          // { title: "Careers", href: "/careers" },
          // { title: "Press", href: "/press" },
          // { title: "News", href: "/news" },
          // { title: "Media Kit", href: "/media" },
          // { title: "Contact", href: "/contact" },
        ],
      },
      // {
      //   title: "Support",
      //   links: [
      //     { title: "Help Center", href: "/help" },
      //     { title: "Booking Support", href: "/support" },
      //     { title: "Travel Alerts", href: "/alerts" },
      //     { title: "Cancellation Policy", href: "/cancellation" },
      //     { title: "Refund Policy", href: "/refund" },
      //     { title: "Contact Support", href: "/contact-support" },
      //   ],
      // },
      // {
      //   title: "Legal",
      //   links: [
      //     { title: "Terms of Service", href: "/terms" },
      //     { title: "Privacy Policy", href: "/privacy" },
      //     { title: "Cookie Policy", href: "/cookies" },
      //     { title: "GDPR", href: "/gdpr" },
      //     { title: "Accessibility", href: "/accessibility" },
      //     { title: "Sitemap", href: "/sitemap" },
      //   ],
      // },
    ],
    social: [
      { name: "Twitter", href: "#", icon: "twitter" },
      { name: "Facebook", href: "#", icon: "facebook" },
      { name: "Instagram", href: "#", icon: "instagram" },
      { name: "LinkedIn", href: "#", icon: "linkedin" },
    ],
    copyright: `©${new Date().getFullYear()}  TravelEase. All rights reserved.`,
    description:
      "Your trusted partner for seamless travel experiences worldwide.",
  },

  // Dashboard
  dashboard: {
    welcome: "Welcome back to your travel dashboard",
    quickActions: [
      { title: "Book a Hotel", href: "/hotels", icon: "hotel" },
      { title: "Find Flights", href: "/flights", icon: "plane" },
      { title: "My Bookings", href: "/bookings", icon: "ticket" },
      { title: "Travel History", href: "/history", icon: "history" },
    ],
    recentBookings: "Recent Bookings",
    upcomingTrips: "Upcoming Trips",
    savedDestinations: "Saved Destinations",
  },

  // Common Actions
  actions: {
    search: "Search",
    book: "Book Now",
    view: "View Details",
    edit: "Edit",
    delete: "Delete",
    cancel: "Cancel",
    save: "Save",
    submit: "Submit",
    back: "Back",
    next: "Next",
    previous: "Previous",
    close: "Close",
    loading: "Loading...",
    error: "Something went wrong",
    success: "Operation completed successfully",
  },

  // Error Messages
  errors: {
    required: "This field is required",
    invalidEmail: "Please enter a valid email address",
    passwordMismatch: "Passwords do not match",
    networkError: "Network error. Please try again.",
    notFound: "Page not found",
    unauthorized: "You are not authorized to access this resource",
    serverError: "Server error. Please try again later.",
  },

  // Success Messages
  success: {
    bookingCreated: "Booking created successfully",
    profileUpdated: "Profile updated successfully",
    passwordChanged: "Password changed successfully",
    loggedIn: "Welcome back!",
    loggedOut: "You have been logged out successfully",
  },
};

// Export individual sections for easier imports
export const {
  brand,
  navigation,
  homepage,
  auth,
  footer,
  dashboard,
  actions,
  errors,
  success,
} = siteData;
