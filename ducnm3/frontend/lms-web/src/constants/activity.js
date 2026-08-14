export const ACTIVITY_RUN = {
  idle: 'idle',
  pending: 'pending',
  success: 'success',
  failed: 'failed',
  skipped: 'skipped',
}

export const ACTIVITY_NODE_KIND = {
  start: 'start',
  action: 'action',
  decision: 'decision',
  end: 'end',
}

export const ACTIVITY_LAYOUT = {
  laneWidth: 176,
  headerHeight: 44,
  rowHeight: 102,
  padX: 16,
  padY: 20,
  actionW: 156,
  actionH: 52,
  decisionW: 140,
  decisionH: 72,
}

export function edgeKey(edge) {
  return `${edge.from}->${edge.to}:${edge.guard ?? ''}`
}

function isPathStep(path, from, to) {
  const index = path.indexOf(from)
  return index >= 0 && path[index + 1] === to
}

export function resolveActivityRun(diagram, { loading, success, error } = {}) {
  const nodeStatus = {}
  const edgeStatus = {}

  for (const node of diagram.nodes) {
    nodeStatus[node.id] = ACTIVITY_RUN.idle
  }

  for (const edge of diagram.edges) {
    edgeStatus[edgeKey(edge)] = ACTIVITY_RUN.idle
  }

  if (loading) {
    for (const id of diagram.pendingPath) {
      nodeStatus[id] = ACTIVITY_RUN.pending
    }

    for (const edge of diagram.edges) {
      if (isPathStep(diagram.pendingPath, edge.from, edge.to)) {
        edgeStatus[edgeKey(edge)] = ACTIVITY_RUN.pending
      }
    }

    return { nodeStatus, edgeStatus }
  }

  if (success) {
    for (const id of Object.keys(nodeStatus)) {
      nodeStatus[id] = diagram.successPath.includes(id)
        ? ACTIVITY_RUN.success
        : ACTIVITY_RUN.skipped
    }

    for (const edge of diagram.edges) {
      const onSuccess = isPathStep(diagram.successPath, edge.from, edge.to)
      edgeStatus[edgeKey(edge)] = onSuccess
        ? ACTIVITY_RUN.success
        : ACTIVITY_RUN.skipped
    }

    return { nodeStatus, edgeStatus }
  }

  if (error?.code) {
    const fail = diagram.failByCode[error.code] ?? diagram.failByCode.default
    const failedId = fail.node
    const failIndex = fail.path.indexOf(failedId)

    for (const id of Object.keys(nodeStatus)) {
      const index = fail.path.indexOf(id)
      if (index < 0) {
        nodeStatus[id] = ACTIVITY_RUN.skipped
      } else if (index >= failIndex) {
        nodeStatus[id] = ACTIVITY_RUN.failed
      } else {
        nodeStatus[id] = ACTIVITY_RUN.success
      }
    }

    for (const edge of diagram.edges) {
      if (!isPathStep(fail.path, edge.from, edge.to)) {
        edgeStatus[edgeKey(edge)] = ACTIVITY_RUN.skipped
        continue
      }

      const toIndex = fail.path.indexOf(edge.to)
      edgeStatus[edgeKey(edge)] =
        toIndex >= failIndex ? ACTIVITY_RUN.failed : ACTIVITY_RUN.success
    }

    return { nodeStatus, edgeStatus, failedId, error }
  }

  return { nodeStatus, edgeStatus }
}
