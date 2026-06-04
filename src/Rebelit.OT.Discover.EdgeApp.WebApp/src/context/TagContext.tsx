import { createContext, useContext, useState } from "react";
import type { Tag } from "../models/Tag";

const STORAGE_KEY = 'tags';

function loadTags(): Tag[] {
  const raw = sessionStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return [];
  }
  return JSON.parse(raw) as Tag[];
}

function saveTags(tags: Tag[]): void {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(tags));
}

type TagContextType = {
  tags: Tag[];
  removeTags: () => void;
  saveTags: (tags: Tag[]) => void;
};

const TagContext = createContext<TagContextType | null>(null);

export function TagsProvider({ children }: { children: React.ReactNode }) {
  const [tags, setTags] = useState<Tag[]>(loadTags);

  return (
    <TagContext.Provider
      value={{
        tags,
        saveTags: (tags: Tag[]) => {
          setTags(tags);
          saveTags(tags);
        },
        removeTags: () => {
          setTags([]);
          saveTags([]);
        }
      }}>
      {children}
    </TagContext.Provider>
  );
}

export function useTags(): TagContextType {
  const ctx = useContext(TagContext);

  if (!ctx) {
    throw new Error('useTags must be used inside TagProvider');
  }

  return ctx;
}
