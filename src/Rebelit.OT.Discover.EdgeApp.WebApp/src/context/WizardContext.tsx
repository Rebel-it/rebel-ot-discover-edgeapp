import { createContext, useContext, useState } from 'react';
import type { WizardStepKey } from '../models/WizardStep';

const STORAGE_KEY = 'wizard-completed-steps';

function loadCompletedSteps(): Set<WizardStepKey> {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return new Set();
  return new Set(JSON.parse(raw) as WizardStepKey[]);
}

function saveCompletedSteps(steps: Set<WizardStepKey>): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify([...steps]));
}

type WizardContextType = {
  completedSteps: Set<WizardStepKey>;
  markStepCompleted: (step: WizardStepKey) => void;
  isStepCompleted: (step: WizardStepKey) => boolean;
};

const WizardContext = createContext<WizardContextType | null>(null);

export function WizardProvider({ children }: { children: React.ReactNode }) {
  const [completedSteps, setCompletedSteps] = useState<Set<WizardStepKey>>(loadCompletedSteps);

  function markStepCompleted(step: WizardStepKey) {
    setCompletedSteps(prev => {
      const next = new Set(prev);
      next.add(step);
      saveCompletedSteps(next);
      return next;
    });
  }

  function isStepCompleted(step: WizardStepKey) {
    return completedSteps.has(step);
  }

  return (
    <WizardContext.Provider value={{ completedSteps, markStepCompleted, isStepCompleted }}>
      {children}
    </WizardContext.Provider>
  );
}

export function useWizard(): WizardContextType {
  const ctx = useContext(WizardContext);

  if (!ctx) {
    throw new Error('useWizard must be used inside WizardProvider');
  }
  
  return ctx;
}
