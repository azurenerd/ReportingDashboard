export const COLORS = {
    shipped:    { bg: '#F0FBF0', bgActive: '#D8F2DA', dot: '#34A853', header: '#E8F5E9', text: '#1B7A28' },
    inProgress: { bg: '#EEF4FE', bgActive: '#DAE8FB', dot: '#0078D4', header: '#E3F2FD', text: '#1565C0' },
    carryover:  { bg: '#FFFDE7', bgActive: '#FFF0B0', dot: '#F4B400', header: '#FFF8E1', text: '#B45309' },
    blocked:    { bg: '#FFF5F5', bgActive: '#FFE4E4', dot: '#EA4335', header: '#FEF2F2', text: '#991B1B' },
    milestone:  { poc: '#F4B400', production: '#34A853', checkpoint: '#999' },
    ui: {
        nowLine: '#EA4335', link: '#0078D4', gridBorder: '#E0E0E0', headerBg: '#F5F5F5',
        timelineBg: '#FAFAFA', bodyText: '#111', subtitleText: '#888', itemText: '#333',
        currentMonthHeaderBg: '#FFF0D0', currentMonthHeaderText: '#C07700',
    },
} as const;