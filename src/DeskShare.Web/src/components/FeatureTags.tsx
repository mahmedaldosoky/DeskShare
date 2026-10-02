import type { DeskFeature } from '../types/models';

export function FeatureTags({ features }: { features: DeskFeature[] }) {
  return (
    <span className="tag-list">
      {features.length === 0 && <span className="tag">Basic desk</span>}
      {features.map((feature) => (
        <span key={feature} className="tag">
          {feature}
        </span>
      ))}
    </span>
  );
}
