export const PAGINATION = {
  types: {
    offset: 'offset',
    cursor: 'cursor',
  },
  defaults: {
    page: 1,
    pageSize: 20,
  },
  limits: {
    pageMin: 1,
    pageSizeMin: 1,
    pageSizeMax: 100,
  },
  pageSizeOptions: [10, 20, 50, 100],
}
