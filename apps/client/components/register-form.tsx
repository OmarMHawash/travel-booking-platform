import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { auth } from "@/lib/site-data";

export function RegisterForm({
  className,
  ...props
}: React.ComponentProps<"div">) {
  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <Card>
        <CardHeader>
          <CardTitle>{auth.register.title}</CardTitle>
          <CardDescription>{auth.register.subtitle}</CardDescription>
        </CardHeader>
        <CardContent>
          <form>
            <div className="flex flex-col gap-6">
              <div className="grid grid-cols-2 gap-4">
                <div className="grid gap-3">
                  <Label htmlFor="firstName">{auth.register.firstName}</Label>
                  <Input
                    id="firstName"
                    type="text"
                    placeholder={auth.register.firstName}
                    required
                  />
                </div>
                <div className="grid gap-3">
                  <Label htmlFor="lastName">{auth.register.lastName}</Label>
                  <Input
                    id="lastName"
                    type="text"
                    placeholder={auth.register.lastName}
                    required
                  />
                </div>
              </div>
              <div className="grid gap-3">
                <Label htmlFor="email">{auth.register.email}</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="Enter your email"
                  required
                />
              </div>
              <div className="grid gap-3">
                <Label htmlFor="password">{auth.register.password}</Label>
                <Input id="password" type="password" required />
              </div>
              <div className="grid gap-3">
                <Label htmlFor="confirmPassword">{auth.register.confirmPassword}</Label>
                <Input id="confirmPassword" type="password" required />
              </div>
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="terms"
                  className="mr-2 h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  required
                />
                <Label htmlFor="terms" className="text-sm">
                  {auth.register.terms}
                </Label>
              </div>
              <div className="flex flex-col gap-3">
                <Button type="submit" className="w-full">
                  {auth.register.registerButton}
                </Button>
                <Button variant="outline" className="w-full">
                  {auth.register.googleRegister}
                </Button>
              </div>
            </div>
            <div className="mt-4 text-center text-sm">
              {auth.register.haveAccount}{" "}
              <a href="#" className="underline underline-offset-4">
                {auth.register.signIn}
              </a>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
