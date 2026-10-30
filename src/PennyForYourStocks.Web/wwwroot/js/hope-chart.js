window.hopeChart = {
  render(canvas, labels, values) {
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
      ctx.fillText('No withdrawal data yet', 10, 20);
      return;
    }
    const max = Math.max(...values, 1);
    const barWidth = width / values.length;
    ctx.fillStyle = '#334155';
    values.forEach((v, i) => {
      const barHeight = (v / max) * (height - 30);
      ctx.fillRect(i * barWidth + 4, height - barHeight - 10, barWidth - 8, barHeight);
    });
    ctx.fillStyle = '#64748b';
    ctx.font = '10px sans-serif';
    labels.forEach((label, i) => {
      ctx.fillText(label, i * barWidth + 4, height - 2);
    });
  },
};
