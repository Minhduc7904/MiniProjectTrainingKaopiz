import {
  ACTIVITY_LAYOUT,
  ACTIVITY_NODE_KIND,
  ACTIVITY_RUN,
  edgeKey,
  resolveActivityRun,
} from '@/constants/activity'
import { UI_LABELS } from '@/constants/ui'
import { ui } from '@/theme'

const nodeClass = {
  [ACTIVITY_RUN.idle]: ui.diagramIdle,
  [ACTIVITY_RUN.pending]: ui.diagramPending,
  [ACTIVITY_RUN.success]: ui.diagramSuccess,
  [ACTIVITY_RUN.failed]: ui.diagramFailed,
  [ACTIVITY_RUN.skipped]: ui.diagramSkipped,
}

const edgeClass = {
  [ACTIVITY_RUN.idle]: ui.diagramEdgeIdle,
  [ACTIVITY_RUN.pending]: ui.diagramEdgePending,
  [ACTIVITY_RUN.success]: ui.diagramEdgeSuccess,
  [ACTIVITY_RUN.failed]: ui.diagramEdgeFailed,
  [ACTIVITY_RUN.skipped]: ui.diagramEdgeSkipped,
}

function nodeBox(node) {
  const x =
    ACTIVITY_LAYOUT.padX +
    node.lane * ACTIVITY_LAYOUT.laneWidth +
    (ACTIVITY_LAYOUT.laneWidth - ACTIVITY_LAYOUT.actionW) / 2
  const y =
    ACTIVITY_LAYOUT.headerHeight +
    ACTIVITY_LAYOUT.padY +
    node.row * ACTIVITY_LAYOUT.rowHeight

  if (node.kind === ACTIVITY_NODE_KIND.start || node.kind === ACTIVITY_NODE_KIND.end) {
    return {
      cx: x + ACTIVITY_LAYOUT.actionW / 2,
      cy: y + ACTIVITY_LAYOUT.actionH / 2,
      x,
      y,
      w: ACTIVITY_LAYOUT.actionW,
      h: ACTIVITY_LAYOUT.actionH,
    }
  }

  if (node.kind === ACTIVITY_NODE_KIND.decision) {
    const w = ACTIVITY_LAYOUT.decisionW
    const h = ACTIVITY_LAYOUT.decisionH
    const dx = x + (ACTIVITY_LAYOUT.actionW - w) / 2
    return { x: dx, y, w, h, cx: dx + w / 2, cy: y + h / 2 }
  }

  return {
    x,
    y,
    w: ACTIVITY_LAYOUT.actionW,
    h: ACTIVITY_LAYOUT.actionH,
    cx: x + ACTIVITY_LAYOUT.actionW / 2,
    cy: y + ACTIVITY_LAYOUT.actionH / 2,
  }
}

function ports(box, node) {
  if (node.kind === ACTIVITY_NODE_KIND.start || node.kind === ACTIVITY_NODE_KIND.end) {
    return {
      n: { x: box.cx, y: box.cy - 9 },
      s: { x: box.cx, y: box.cy + 9 },
      e: { x: box.cx + 9, y: box.cy },
      w: { x: box.cx - 9, y: box.cy },
    }
  }

  return {
    n: { x: box.cx, y: box.y },
    s: { x: box.cx, y: box.y + box.h },
    e: { x: box.x + box.w, y: box.cy },
    w: { x: box.x, y: box.cy },
  }
}

function edgePath(fromBox, toBox, fromNode, toNode) {
  const from = ports(fromBox, fromNode)
  const to = ports(toBox, toNode)
  const goingRight = toBox.cx - fromBox.cx > 24
  const goingLeft = fromBox.cx - toBox.cx > 24
  const start = goingRight ? from.e : goingLeft ? from.e : from.s
  const end = goingRight || goingLeft ? to.w : to.n
  const midY = (start.y + end.y) / 2

  if (goingRight || goingLeft) {
    return `M ${start.x} ${start.y} C ${start.x + 36} ${start.y}, ${end.x - 36} ${end.y}, ${end.x} ${end.y}`
  }

  return `M ${start.x} ${start.y} C ${start.x} ${midY}, ${end.x} ${midY}, ${end.x} ${end.y}`
}

function diamondPoints(box) {
  const { x, y, w, h } = box
  return `${x + w / 2},${y} ${x + w},${y + h / 2} ${x + w / 2},${y + h} ${x},${y + h / 2}`
}

function wrapLabel(label, max = 22) {
  if (label.length <= max) {
    return [label]
  }

  const words = label.split(' ')
  const lines = []
  let current = ''

  for (const word of words) {
    const next = current ? `${current} ${word}` : word
    if (next.length > max && current) {
      lines.push(current)
      current = word
    } else {
      current = next
    }
  }

  if (current) {
    lines.push(current)
  }

  return lines.slice(0, 3)
}

function NodeShape({ node, box, status }) {
  const className = nodeClass[status] ?? ui.diagramIdle

  if (node.kind === ACTIVITY_NODE_KIND.start) {
    return <circle cx={box.cx} cy={box.cy} r={8} className={className} />
  }

  if (node.kind === ACTIVITY_NODE_KIND.end) {
    return (
      <g>
        <circle cx={box.cx} cy={box.cy} r={10} className={`${className} fill-none`} />
        <circle cx={box.cx} cy={box.cy} r={5} className={className} />
      </g>
    )
  }

  if (node.kind === ACTIVITY_NODE_KIND.decision) {
    return <polygon points={diamondPoints(box)} className={className} />
  }

  return (
    <rect
      x={box.x}
      y={box.y}
      width={box.w}
      height={box.h}
      rx={10}
      className={className}
    />
  )
}

export function ActivityDiagram({ diagram, run }) {
  const resolved = resolveActivityRun(diagram, run)
  const boxes = Object.fromEntries(
    diagram.nodes.map((node) => [node.id, nodeBox(node)]),
  )
  const maxRow = Math.max(...diagram.nodes.map((node) => node.row))
  const width = ACTIVITY_LAYOUT.padX * 2 + diagram.lanes.length * ACTIVITY_LAYOUT.laneWidth
  const height =
    ACTIVITY_LAYOUT.headerHeight +
    ACTIVITY_LAYOUT.padY * 2 +
    (maxRow + 1) * ACTIVITY_LAYOUT.rowHeight

  const failedNode = diagram.nodes.find((node) => node.id === resolved.failedId)
  const statusLabel = run?.loading
    ? UI_LABELS.loading
    : run?.error
      ? `${run.error.code}${failedNode ? ` · ${failedNode.label}` : ''}`
      : run?.success
        ? UI_LABELS.umlSuccess
        : UI_LABELS.umlIdle

  return (
    <div className="flex h-full min-h-0 flex-col">
      <div className={`flex shrink-0 items-center justify-between gap-3 px-5 py-3 ${ui.hairlineB}`}>
        <div>
          <p className={`font-mono text-[12px] ${ui.title}`}>
            {diagram.method} {diagram.path}
          </p>
          <p className={`mt-0.5 text-[12px] ${ui.body}`}>{diagram.title}</p>
        </div>
        <p
          className={`rounded-full px-2.5 py-1 text-[11px] font-medium ${
            run?.error
              ? ui.badgeDanger
              : run?.success
                ? ui.badgeSuccess
                : run?.loading
                  ? ui.badgeWarning
                  : ui.badgeMuted
          }`}
        >
          {statusLabel}
        </p>
      </div>
      <div className="min-h-0 flex-1 overflow-auto px-4 py-4">
        <svg
          viewBox={`0 0 ${width} ${height}`}
          width={width}
          height={height}
          className="max-w-none"
          role="img"
          aria-label={`UML Activity Diagram ${diagram.method} ${diagram.path}`}
        >
          <defs>
            <marker
              id="activity-arrow"
              markerWidth="8"
              markerHeight="8"
              refX="6"
              refY="4"
              orient="auto"
            >
              <path d="M0,0 L8,4 L0,8 Z" className="fill-fg-muted" />
            </marker>
          </defs>
          {diagram.lanes.map((lane, index) => {
            const x = ACTIVITY_LAYOUT.padX + index * ACTIVITY_LAYOUT.laneWidth
            const isFault = index === diagram.lanes.length - 1

            return (
              <g key={lane}>
                <rect
                  x={x}
                  y={0}
                  width={ACTIVITY_LAYOUT.laneWidth - 8}
                  height={height}
                  rx={10}
                  className={isFault ? ui.diagramLaneFault : ui.diagramLane}
                />
                <text
                  x={x + (ACTIVITY_LAYOUT.laneWidth - 8) / 2}
                  y={26}
                  textAnchor="middle"
                  className="fill-fg-subtle text-[11px] font-medium tracking-wide uppercase"
                >
                  {lane}
                </text>
              </g>
            )
          })}
          {diagram.edges.map((edge) => {
            const fromNode = diagram.nodes.find((node) => node.id === edge.from)
            const toNode = diagram.nodes.find((node) => node.id === edge.to)
            const d = edgePath(boxes[edge.from], boxes[edge.to], fromNode, toNode)
            const status = resolved.edgeStatus[edgeKey(edge)]
            const fromBox = boxes[edge.from]
            const toBox = boxes[edge.to]
            const labelX = (fromBox.cx + toBox.cx) / 2
            const labelY = (fromBox.cy + toBox.cy) / 2 - 8

            return (
              <g key={edgeKey(edge)}>
                <path
                  d={d}
                  fill="none"
                  strokeLinecap="round"
                  markerEnd="url(#activity-arrow)"
                  className={edgeClass[status] ?? ui.diagramEdgeIdle}
                />
                {edge.guard ? (
                  <text
                    x={labelX}
                    y={labelY}
                    textAnchor="middle"
                    className="fill-fg-muted text-[10px]"
                  >
                    [{edge.guard}]
                  </text>
                ) : null}
              </g>
            )
          })}
          {diagram.nodes.map((node) => {
            const box = boxes[node.id]
            const status = resolved.nodeStatus[node.id]
            const lines = wrapLabel(node.label)
            const textY =
              node.kind === ACTIVITY_NODE_KIND.decision
                ? box.cy - ((lines.length - 1) * 10) / 2
                : box.cy - ((lines.length - 1) * 11) / 2

            return (
              <g key={node.id}>
                <NodeShape node={node} box={box} status={status} />
                {node.kind === ACTIVITY_NODE_KIND.end ? (
                  <text
                    x={box.cx}
                    y={box.cy + 24}
                    textAnchor="middle"
                    className="fill-fg-subtle text-[10px]"
                  >
                    {node.label}
                  </text>
                ) : null}
                {node.kind === ACTIVITY_NODE_KIND.action ||
                node.kind === ACTIVITY_NODE_KIND.decision ? (
                  <text
                    x={box.cx}
                    y={textY}
                    textAnchor="middle"
                    className="fill-fg text-[11px] font-medium"
                  >
                    {lines.map((line, lineIndex) => (
                      <tspan
                        key={`${node.id}-${lineIndex}`}
                        x={box.cx}
                        dy={lineIndex === 0 ? 0 : 12}
                      >
                        {line}
                      </tspan>
                    ))}
                  </text>
                ) : null}
              </g>
            )
          })}
        </svg>
        {diagram.notes?.empty ? (
          <p className={`mt-3 text-[12px] ${ui.caption}`}>{diagram.notes.empty}</p>
        ) : null}
        {run?.error ? (
          <p className={`mt-2 text-[13px] ${ui.body}`}>
            {run.error.message}
          </p>
        ) : null}
      </div>
    </div>
  )
}
