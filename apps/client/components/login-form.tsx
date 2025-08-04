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

export function LoginForm({
  className,
  ...props
}: React.ComponentProps<"div">) {
  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <Card>
        <CardHeader>
          <CardTitle>{auth.login.title}</CardTitle>
          <CardDescription>{auth.login.subtitle}</CardDescription>
        </CardHeader>
        <CardContent>
          <form>
            <div className="flex flex-col gap-6">
              <div className="grid gap-3">
                <Label htmlFor="email">{auth.login.email}</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder={auth.login.emailPlaceholder}
                  required
                />
              </div>
              <div className="grid gap-3">
                <div className="flex items-center">
                  <Label htmlFor="password">{auth.login.password}</Label>
                  <a
                    href="#"
                    className="ml-auto inline-block text-sm underline-offset-4 hover:underline"
                  >
                    {auth.login.forgotPassword}
                  </a>
                </div>
                <Input id="password" type="password" required />
              </div>
              <div className="flex flex-col gap-3">
                <Button type="submit" className="w-full">
                  {auth.login.loginButton}
                </Button>
                <Button variant="outline" className="w-full">
                  {auth.login.googleLogin}
                </Button>
              </div>
            </div>
            <div className="mt-4 text-center text-sm">
              {auth.login.noAccount}{" "}
              <a href="#" className="underline underline-offset-4">
                {auth.login.signUp}
              </a>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
