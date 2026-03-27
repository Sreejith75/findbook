export type NavigationItem = {
  href: string;
  label: string;
  shortLabel: string;
  title: string;
  badge?: string;
};

export type NavigationSection = {
  label: string;
  items: NavigationItem[];
};

export const navigationSections: NavigationSection[] = [
  {
    label: "Menu",
    items: [
      { href: "/", label: "Home", shortLabel: "HM", title: "Home" },
      {
        href: "/catalog",
        label: "Browse Catalog",
        shortLabel: "CT",
        title: "Browse Catalog",
      },
      {
        href: "/rentals",
        label: "My Rentals",
        shortLabel: "RT",
        title: "My Rentals",
        badge: "3",
      },
      {
        href: "/deliveries",
        label: "Deliveries",
        shortLabel: "DV",
        title: "Deliveries",
      },
      {
        href: "/libraries",
        label: "Libraries",
        shortLabel: "LB",
        title: "Partner Libraries",
      },
    ],
  },
  {
    label: "Account",
    items: [
      {
        href: "/profile",
        label: "Profile",
        shortLabel: "PF",
        title: "My Profile",
      },
      {
        href: "/admin",
        label: "Admin Panel",
        shortLabel: "AD",
        title: "Admin Panel",
      },
    ],
  },
];

export const routeTitles = navigationSections.reduce<Record<string, string>>(
  (accumulator, section) => {
    for (const item of section.items) {
      accumulator[item.href] = item.title;
    }

    return accumulator;
  },
  {},
);
