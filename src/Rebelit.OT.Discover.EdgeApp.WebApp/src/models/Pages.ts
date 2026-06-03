import type { WizardStepKey } from './WizardStep';

export const Pages: Record<WizardStepKey, string> = {
  start: "/",
  login: "/login",
  plcConnect: "/plcConnect",
  source: "/source",
  variables: "/variables",
  tags: "/tags",
  deviceConfig: "/deviceConfig",
  final: "/final",
}