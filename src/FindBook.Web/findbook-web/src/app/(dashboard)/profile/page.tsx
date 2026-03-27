import { ProfileOverview } from "@/features/book-rental";
import { getProfileOverviewData } from "@/features/book-rental/services/book-rental.service";

export const dynamic = "force-dynamic";

export default async function ProfilePage() {
  const data = await getProfileOverviewData();
  return <ProfileOverview data={data} />;
}
