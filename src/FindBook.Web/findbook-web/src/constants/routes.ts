export type NavigationItem = {
  allowedRoles?: string[];
  href: string;
  label: string;
  shortLabel: string;
  title: string;
  badge?: string;
  children?: NavigationItem[];
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
        allowedRoles: ["User"],
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
        allowedRoles: ["Admin", "SuperAdmin"],
        children: [
          {
            href: "/admin/users",
            label: "Users",
            shortLabel: "US",
            title: "Admin Users",
            allowedRoles: ["Admin", "SuperAdmin"],
          },
          {
            href: "/admin/categories",
            label: "Categories",
            shortLabel: "CT",
            title: "Admin Categories",
            allowedRoles: ["Admin", "SuperAdmin"],
          },
          {
            href: "/admin/libraries",
            label: "Libraries",
            shortLabel: "LB",
            title: "Admin Libraries",
            allowedRoles: ["Admin", "SuperAdmin"],
          },
          {
            href: "/admin/books",
            label: "Books",
            shortLabel: "BK",
            title: "Admin Books",
            allowedRoles: ["Admin", "SuperAdmin"],
          },
          {
            href: "/admin/delivery",
            label: "Delivery",
            shortLabel: "DV",
            title: "Admin Delivery Tasks",
            allowedRoles: ["Admin", "SuperAdmin"],
          },
        ],
      },
    ],
  },
];

export const routeTitles = navigationSections.reduce<Record<string, string>>(
  (accumulator, section) => {
    for (const item of section.items) {
      accumulator[item.href] = item.title;
      for (const child of item.children ?? []) {
        accumulator[child.href] = child.title;
      }
    }

    return accumulator;
  },
  {},
);
