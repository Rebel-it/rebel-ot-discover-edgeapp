export type TagLogEvent = 'interval' | 'change' | 'trigger'

export type TagVariable = {
  publicId: string
}

export type Tag = {
  logEvent: TagLogEvent
  loggingInterval: string
  name: string
  onChangeExpiry: string | null
  retentionPolicy: string
  slug: string
  variable: TagVariable
  edgeAggregator: string | null
}

export function getFormulaLabel(tag: Tag) {
  if (tag.logEvent === "change") {
    return tag.onChangeExpiry === "1h" ? "value_changes_hourly" : "value_changes_only";
  }

  return tag.edgeAggregator ?? "";
}

export function getTagKey(tag: Tag) {
  return `${tag.slug}-${tag.variable.publicId}`;
}