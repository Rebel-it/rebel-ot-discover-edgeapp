export const WizardStep = {
    start: {
      title: "",
      description: "",
    },
    login: {
      title: "Log in",
      description: "For security reasons, you can not use personal user credentials. Instead, create a dedicated [App Name] service account. This is required because our application will write data to your cloud environment and needs appropriate access.\n\nAfter setup, you can delete this service account if needed.\n\nGo to:\nhttps://portal.ixon.cloud/admin/service-accounts and add a new Service Account\n\nOnce you create the service account, you will receive an access token.\nMake sure to copy both required values immediately\nIf you close or navigate away, you will not be able to retrieve the access token again",
    },
    plcConnect: {
      title: "PLC connection",
      description: "Enter the OPC server address of your machine. This information is not provided automatically, so you will need to obtain it yourself.\n\nIf your server requires authentication, you can enable the checkbox and provide a username and password.",
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