"""Conservative XY-cut reading order for horizontal CV text, without an LLM."""


def reading_order(blocks):
    if len(blocks) < 2:
        return blocks
    # Split only at whitespace spanning the entire current region. This keeps
    # full-width headings above their columns instead of merging adjacent lines.
    for start, end, minimum in ((0, 2, 24), (1, 3, 12)):
        intervals = sorted((b["box"][start], b["box"][end]) for b in blocks)
        edge = intervals[0][1]
        gaps = []
        for left, right in intervals[1:]:
            if left - edge >= minimum:
                gaps.append((left - edge, (left + edge) / 2))
            edge = max(edge, right)
        if gaps:
            _, cut = max(gaps)
            first = [b for b in blocks if b["box"][end] <= cut]
            second = [b for b in blocks if b["box"][start] >= cut]
            if first and second and len(first) + len(second) == len(blocks):
                return reading_order(first) + reading_order(second)
    return sorted(blocks, key=lambda b: (b["box"][1], b["box"][0]))
