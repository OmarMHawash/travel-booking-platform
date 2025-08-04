import { AppSidebar } from "@/components/app-sidebar";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { Separator } from "@/components/ui/separator";
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar";
import { dashboard } from "@/lib/site-data";

export default function Page() {
  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
          <div className="flex items-center gap-2 px-4">
            <SidebarTrigger className="-ml-1" />
            <Separator
              orientation="vertical"
              className="mr-2 data-[orientation=vertical]:h-4"
            />
            <Breadcrumb>
              <BreadcrumbList>
                <BreadcrumbItem className="hidden md:block">
                  <BreadcrumbLink href="/dashboard">Dashboard</BreadcrumbLink>
                </BreadcrumbItem>
                <BreadcrumbSeparator className="hidden md:block" />
                <BreadcrumbItem>
                  <BreadcrumbPage>Overview</BreadcrumbPage>
                </BreadcrumbItem>
              </BreadcrumbList>
            </Breadcrumb>
          </div>
        </header>
        <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
          {/* Welcome Section */}
          <div className="bg-muted/50 p-6 rounded-xl">
            <h1 className="text-2xl font-bold mb-2">{dashboard.welcome}</h1>
            <p className="text-muted-foreground">
              Manage your travel bookings and preferences
            </p>
          </div>

          {/* Quick Actions */}
          <div className="grid auto-rows-min gap-4 md:grid-cols-2 lg:grid-cols-4">
            {dashboard.quickActions.map((action, index) => (
              <div
                key={index}
                className="bg-muted/50 p-4 rounded-xl hover:bg-muted/70 transition-colors cursor-pointer"
              >
                <h3 className="font-semibold mb-2">{action.title}</h3>
                <p className="text-sm text-muted-foreground">
                  Click to {action.title.toLowerCase()}
                </p>
              </div>
            ))}
          </div>

          {/* Recent Bookings */}
          <div className="bg-muted/50 p-6 rounded-xl">
            <h2 className="text-xl font-semibold mb-4">
              {dashboard.recentBookings}
            </h2>
            <p className="text-muted-foreground">No recent bookings found</p>
          </div>

          {/* Upcoming Trips */}
          <div className="bg-muted/50 p-6 rounded-xl">
            <h2 className="text-xl font-semibold mb-4">
              {dashboard.upcomingTrips}
            </h2>
            <p className="text-muted-foreground">No upcoming trips planned</p>
          </div>
        </div>
      </SidebarInset>
    </SidebarProvider>
  );
}
