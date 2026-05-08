export function PostMediaGrid({ media = [] }) {
  if (!media.length) return null;
  if (media.length === 1) {
    return <img src={media[0].url} alt={media[0].alt || 'Post'} className="w-full h-64 sm:h-80 object-cover rounded-xl border border-slate-100" />;
  }
  return (
    <div className="grid grid-cols-2 gap-2">
      {media.slice(0, 4).map((item) => (
        <img key={item.url} src={item.url} alt={item.alt || 'Post'} className="w-full h-40 object-cover rounded-xl border border-slate-100" />
      ))}
    </div>
  );
}


