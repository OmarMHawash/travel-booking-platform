# TravelEase - Travel Booking Platform

A modern travel booking platform built with Next.js, TypeScript, and Tailwind CSS.

## Features

- **Personalized Travel Experience**: Custom branding and content for TravelEase
- **Responsive Design**: Mobile-first approach with modern UI components
- **Authentication**: Login and registration forms with Google integration
- **Dashboard**: User dashboard for managing bookings and preferences
- **Navigation**: Dynamic navigation with travel-specific menu items
- **Footer**: Comprehensive footer with travel services and support links

## Static Data Structure

The application uses a centralized static data file (`lib/site-data.ts`) that contains:

- **Brand Information**: Company name, tagline, and description
- **Navigation**: Main and user navigation items
- **Homepage Content**: Hero section, features, and statistics
- **Authentication**: Login and registration form text
- **Footer**: Footer sections and links
- **Dashboard**: Dashboard content and quick actions
- **Common Actions**: Reusable action text
- **Error Messages**: Standardized error messages
- **Success Messages**: Success notification text

## Key Components

### Updated Components

- `components/login-form.tsx` - Uses authentication data from static file
- `components/navbar-04/logo.tsx` - Displays TravelEase branding
- `components/navbar-04/nav-menu.tsx` - Dynamic navigation menu
- `components/footer-02/footer-02.tsx` - Travel-specific footer content
- `components/app-sidebar.tsx` - Dashboard sidebar with travel services

### Updated Pages

- `app/(home)/page.tsx` - Homepage with hero section and features
- `app/dashboard/page.tsx` - User dashboard with travel management
- `app/layout.tsx` - Updated metadata with brand information

## Getting Started

1. Install dependencies:

   ```bash
   npm install
   ```

2. Run the development server:

   ```bash
   npm run dev
   ```

3. Open [http://localhost:3000](http://localhost:3000) in your browser.

## Customization

To modify the content, edit the `lib/site-data.ts` file. This centralized approach makes it easy to:

- Update branding and company information
- Modify navigation items
- Change homepage content
- Update form labels and messages
- Customize footer links and sections

## Technology Stack

- **Next.js 14** - React framework with App Router
- **TypeScript** - Type-safe JavaScript
- **Tailwind CSS** - Utility-first CSS framework
- **shadcn/ui** - Modern UI component library
- **Lucide React** - Icon library

## Project Structure

```
travel-bp-client/
├── app/                    # Next.js app directory
│   ├── (home)/            # Homepage routes
│   ├── dashboard/         # Dashboard pages
│   └── layout.tsx         # Root layout
├── components/            # React components
│   ├── ui/               # shadcn/ui components
│   ├── navbar-04/        # Navigation components
│   ├── footer-02/        # Footer components
│   └── app-sidebar.tsx   # Dashboard sidebar
├── lib/                  # Utility functions
│   ├── site-data.ts      # Static content data
│   └── utils.ts          # Utility functions
└── public/               # Static assets
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Update the static data file if needed
5. Submit a pull request

## License

This project is licensed under the MIT License.
