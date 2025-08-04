import Footer01Page from "@/components/footer-02/footer-02";
import Navbar04Page from "@/components/navbar-04/navbar-04";

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <>
      <Navbar04Page />
      {children}
      <Footer01Page />
    </>
  );
}
