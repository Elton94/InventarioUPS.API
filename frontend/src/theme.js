import { createTheme } from '@mui/material/styles'

const theme = createTheme({
  typography: {
    fontFamily: '"Segoe UI", Roboto, Arial, sans-serif',
    button: { textTransform: 'none', fontWeight: 600 },
  },
  components: {
    MuiButton: {
      styleOverrides: { root: { borderRadius: '14px' } },
    },
    MuiOutlinedInput: {
      styleOverrides: { root: { borderRadius: '12px' } },
    },
    MuiPaper: {
      styleOverrides: { rounded: { borderRadius: '14px' } },
    },
    MuiCard: {
      styleOverrides: { root: { borderRadius: '16px' } },
    },
    MuiDialog: {
      styleOverrides: { paper: { borderRadius: '20px' } },
    },
    MuiAlert: {
      styleOverrides: { root: { borderRadius: '12px' } },
    },
    MuiMenu: {
      styleOverrides: { paper: { borderRadius: '12px' } },
    },
    MuiTooltip: {
      styleOverrides: { tooltip: { borderRadius: '8px' } },
    },
    MuiTableContainer: {
      styleOverrides: { root: { borderRadius: '12px' } },
    },
    MuiTab: {
      styleOverrides: { root: { borderRadius: '12px', textTransform: 'none' } },
    },
    MuiAccordion: {
      styleOverrides: {
        root: {
          '&:first-of-type': { borderTopLeftRadius: '14px', borderTopRightRadius: '14px' },
          '&:last-of-type': { borderBottomLeftRadius: '14px', borderBottomRightRadius: '14px' },
        },
      },
    },
  },
})

export default theme
