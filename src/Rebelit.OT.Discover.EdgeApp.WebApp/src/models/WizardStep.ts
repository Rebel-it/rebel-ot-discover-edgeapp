import { Pages } from "./Pages";

export const WizardStep = {
    start: {
      title: "",
      description: "",
    },
    login: {
      title: "Log in",
      description: "For security reasons, it is strongly recommended not to use personal user credentials. Instead, create a dedicated service account. This is required because the application will write data to your cloud environment and needs appropriate access.\n\nAfter setup, you can delete this service account if needed.\n\nGo to: Admin → Integrations → Service Accounts → Add New Service Account.\n\nOnce you create the service account, you will receive an access token. Make sure to copy both required values immediately — if you close or navigate away, you will not be able to retrieve the access token again.",
    },
    plcConnect: {
      title: "PLC connection",
      description: "You must be connected to your machine via VPN. This requirement is also indicated on the start screen. \n\nEnter the endpoint URL or IP address of your machine. This information is not provided automatically, so you will need to obtain it yourself.\n\nYou can also optionally provide a username and password for authentication. If required, simply enable the checkbox and fill in the credentials.",
    },
    source: {
      title: "Create data source",
      description: "A data source is used to group variables together. Here, you can create a new data source, which will then appear in your IXON Manager.\n\nThe name field is prefilled with a suggested placeholder, which you can keep or modify as needed. If you enter the name of an existing data source, it will be updated and extended instead of creating a new one.",
    },
    variables: {
      title: "Synchronize variables",
      description: "",
    },
    tags: {
      title: "Tags",
      description: "In this step, you select the variables that are relevant to you and that you want to use.\n\nWe automatically populate default values to help you get started quickly. These values are based on common use cases and are often a good starting point. \n\nHowever, please note that you have more advanced configuration options available in the portal. If you want to create custom tags or adjust the setup in more detail, you can do so there.\n\nThis is especially useful if you are not sure what values to configure or if you need more flexibility beyond the default settings.",
    },
    deviceConfig: {
      title: "Device configuration",
      description: "All tags that you select will be sent to your IXON device. This step is essential to enable data flow from your machine to the cloud.\n\nWithout this configuration, the IXON device will not know which data to collect, and your dashboards will not display any information.\n\nTo give you clarity, we provide an overview of the tags that will be pushed before completing this step. This allows you to review and confirm your selection.",
    },
    final: {
      title: "Finish",
      description: "",
    }

} as const;

export type WizardStep = typeof WizardStep[keyof typeof WizardStep];
export type WizardStepKey = keyof typeof WizardStep;

export const WizardStepOrder: WizardStepKey[] = [
    "login",
    "plcConnect",
    "source",
    "variables",
    "tags",
    "deviceConfig",
    "final",
];