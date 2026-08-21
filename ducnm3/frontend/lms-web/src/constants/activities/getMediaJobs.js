import { ACTIVITY_NODE_KIND } from '@/constants/activity'
import { API_ERROR_CODES } from '@/constants/apiErrorCodes'
import { API_ROUTES } from '@/constants/apiRoutes'

export const GET_MEDIA_JOBS_ACTIVITY = {
  id: 'getMediaJobs', title: 'Liệt kê Media job', method: 'GET', path: API_ROUTES.media.jobs,
  lanes: ['Admin', 'Gateway', 'Media Service', 'Repository', 'MySQL', 'Fault'],
  nodes: [
    { id: 'start', kind: ACTIVITY_NODE_KIND.start, label: '', lane: 0, row: 0 },
    { id: 'request', kind: ACTIVITY_NODE_KIND.action, label: 'GET query filters', lane: 0, row: 1 },
    { id: 'gateway', kind: ACTIVITY_NODE_KIND.action, label: 'Forward request', lane: 1, row: 2 },
    { id: 'validate', kind: ACTIVITY_NODE_KIND.action, label: 'Validate ADMIN + query', lane: 2, row: 3 },
    { id: 'invalid', kind: ACTIVITY_NODE_KIND.action, label: '400 invalid query/actor', lane: 5, row: 4 },
    { id: 'repo', kind: ACTIVITY_NODE_KIND.action, label: 'Count + stable page', lane: 3, row: 5 },
    { id: 'db', kind: ACTIVITY_NODE_KIND.action, label: 'SELECT background jobs', lane: 4, row: 6 },
    { id: 'ok', kind: ACTIVITY_NODE_KIND.action, label: '200 data + pagination', lane: 2, row: 7 },
    { id: 'end', kind: ACTIVITY_NODE_KIND.end, label: 'Success', lane: 0, row: 8 },
    { id: 'fail', kind: ACTIVITY_NODE_KIND.end, label: 'Fail', lane: 5, row: 8 },
  ],
  edges: [
    { from: 'start', to: 'request' }, { from: 'request', to: 'gateway' },
    { from: 'gateway', to: 'validate' }, { from: 'validate', to: 'repo' },
    { from: 'validate', to: 'invalid', guard: 'Không hợp lệ', outcome: 'fail' },
    { from: 'invalid', to: 'fail', outcome: 'fail' }, { from: 'repo', to: 'db' },
    { from: 'db', to: 'ok' }, { from: 'ok', to: 'end' },
  ],
  pendingPath: ['start', 'request', 'gateway'],
  successPath: ['start', 'request', 'gateway', 'validate', 'repo', 'db', 'ok', 'end'],
  failByCode: {
    [API_ERROR_CODES.invalidMedia]: { node: 'invalid', path: ['start', 'request', 'gateway', 'validate', 'invalid', 'fail'] },
    [API_ERROR_CODES.invalidActorType]: { node: 'invalid', path: ['start', 'request', 'gateway', 'validate', 'invalid', 'fail'] },
    default: { node: 'invalid', path: ['start', 'request', 'gateway', 'validate', 'invalid', 'fail'] },
  },
  notes: { empty: '200 + data [] không phải lỗi.' },
}
