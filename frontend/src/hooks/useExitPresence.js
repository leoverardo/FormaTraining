import { useEffect, useState } from 'react';
import { useMotionPreference } from '../features/motion/useMotionPreference';

// Matches --duration-exit in index.css; cancel removal if reopened mid-transition.
export function useExitPresence(open) {
  const reduced = useMotionPreference();
  const [present, setPresent] = useState(open);

  useEffect(() => {
    if (open) {
      setPresent(true);
      return undefined;
    }
    if (reduced) {
      setPresent(false);
      return undefined;
    }
    const timeout = setTimeout(() => setPresent(false), 160);
    return () => clearTimeout(timeout);
  }, [open, reduced]);

  return open || (!reduced && present);
}
