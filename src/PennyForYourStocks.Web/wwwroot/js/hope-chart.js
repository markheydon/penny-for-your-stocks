window.hopeChart = {
  render(canvas, labels, values, hopeBandLower, hopeBandUpper) {
    if (!canvas || !canvas.getContext) {
      return;
    }
    const ctx = canvas.getContext('2d');
    const width = canvas.clientWidth || 480;
    const height = canvas.clientHeight || 240;
    canvas.width = width;
    canvas.height = height;
    ctx.clearRect(0, 0, width, height);
    if (!values.length) {
      return;
    }

    const lower = hopeBandLower ?? 100;
    const upper = hopeBandUpper ?? 200;
    const chartTop = 16;
    const chartBottom = height - 24;
    const chartHeight = chartBottom - chartTop;
    const max = Math.max(...values, upper, 1);
    const scaleY = (amount) => chartBottom - (amount / max) * chartHeight;

    ctx.strokeStyle = '#cbd5e1';
    ctx.setLineDash([4, 4]);
    ctx.lineWidth = 1;
    [lower, upper].forEach((band) => {
      const y = scaleY(band);
      ctx.beginPath();
      ctx.moveTo(0, y);
      ctx.lineTo(width, y);
      ctx.stroke();
    });
    ctx.setLineDash([]);

    ctx.fillStyle = '#94a3b8';
    ctx.font = '10px sans-serif';
    ctx.fillText(`Hope band £${lower}–£${upper}`, 4, 12);

    const barWidth = width / values.length;
    ctx.fillStyle = '#334155';
    values.forEach((v, i) => {
      const y = scaleY(v);
      const barHeight = chartBottom - y;
      ctx.fillRect(i * barWidth + 4, y, barWidth - 8, barHeight);
    });

    ctx.fillStyle = '#64748b';
    labels.forEach((label, i) => {
      ctx.fillText(label, i * barWidth + 4, height - 6);
    });
  },
};
