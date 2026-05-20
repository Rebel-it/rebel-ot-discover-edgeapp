export type Option = { label: string; value: string }

export const LOGGING_ON_OPTIONS = ['Interval', 'Change']

export const INTERVAL_OPTIONS: Option[] = [
    { label: 'Every 200 milliseconds', value: '200ms' },
    { label: 'Every 500 milliseconds', value: '500ms' },
    { label: 'Every second',           value: '1s'    },
    { label: 'Every 10 seconds',       value: '10s'   },
    { label: 'Every 30 seconds',       value: '30s'   },
    { label: 'Every minute',           value: '1m'    },
    { label: 'Every 5 minutes',        value: '5m'    },
    { label: 'Every 15 minutes',       value: '15m'   },
    { label: 'Every 30 minutes',       value: '30m'   },
    { label: 'Every hour',             value: '1h'    },
    { label: 'Custom',                 value: 'custom'},
]

export const RATE_LIMIT_OPTIONS: Option[] = [
    { label: 'Up to 20000 values per hour', value: '180ms'  },
    { label: 'Up to 5000 values per hour',  value: '720ms'  },
    { label: 'Up to 1000 values per hour',  value: '3600ms' },
    { label: 'Up to 500 values per hour',   value: '7200ms' },
    { label: 'Up to 100 values per hour',   value: '36s'    },
    { label: 'Up to 50 values per hour',    value: '72s'    },
    { label: 'Up to 25 values per hour',    value: '144s'   },
    { label: 'Up to 10 values per hour',    value: '6m'     },
    { label: 'Up to 5 values per hour',     value: '12m'    },
]

export const SPECIFICATION_OPTIONS: Option[] = [
    { label: 'On value changes only (default)',                    value: 'value_changes_only'    },
    { label: 'On value changes and each hour of unchanged value',  value: 'value_changes_hourly'  },
]

export const FORMULA_OPTIONS: Option[] = [
    { label: 'LAST (default)', value: '' },
]

export const RETENTION_OPTIONS: Option[] = [
    { label: '3 days',   value: '3d' },
    { label: '7 days',   value: '7d' },
    { label: '2 weeks',  value: '14d' },
    { label: '6 months', value: '26w' },
    { label: '2 years',  value: '104w' },
    { label: '5 years',  value: '260w' },
    { label: '7 years',  value: '364w' },
]
