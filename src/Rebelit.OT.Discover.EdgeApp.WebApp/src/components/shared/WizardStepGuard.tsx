import { Navigate, Outlet } from 'react-router-dom';
import { useWizard } from '../../context/WizardContext';
import type { WizardStepKey } from '../../models/WizardStep';
import { Pages } from '../../models/Pages';

type Props = {
    requiredStep: WizardStepKey;
}

export default function WizardStepGuard({ requiredStep }: Readonly<Props>) {
    const { isStepCompleted } = useWizard();

    if (!isStepCompleted(requiredStep)) {
        return <Navigate to={Pages[requiredStep]} replace />;
    }

    return <Outlet />;
}
