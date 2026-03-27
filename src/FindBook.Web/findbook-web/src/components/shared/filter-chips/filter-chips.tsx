"use client";

import { useState } from "react";

import { cn } from "@/utils/cn";

type FilterChipsProps = {
  defaultValue?: string;
  items: string[];
  onChange?: (value: string) => void;
};

export function FilterChips({
  defaultValue,
  items,
  onChange,
}: FilterChipsProps) {
  const [activeItem, setActiveItem] = useState<string>(
    defaultValue ?? items[0] ?? "",
  );

  return (
    <div className="chip-row">
      {items.map((item) => (
        <button
          key={item}
          className={cn("chip", activeItem === item && "chip-active")}
          onClick={() => {
            setActiveItem(item);
            onChange?.(item);
          }}
          type="button"
        >
          {item}
        </button>
      ))}
    </div>
  );
}
