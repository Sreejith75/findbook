import { redirect } from "next/navigation";

import { HomeOverview } from "@/features/book-rental";
import { getHomeOverviewData } from "@/features/book-rental/services/book-rental.service";
import { getServerSession } from "@/lib/auth-session";
import { getDefaultRouteForRole } from "@/lib/role-routing";

export const dynamic = "force-dynamic";

export default async function HomePage() {
  const session = await getServerSession();
  if (session.role !== "User") {
    redirect(getDefaultRouteForRole(session.role));
  }

  const data = await getHomeOverviewData();
  return <HomeOverview data={data} />;
}
