export interface AboutContent {
  aboutTr: string;
  aboutEn: string;

  missionTr: string;
  missionEn: string;

  visionTr: string;
  visionEn: string;
}

export interface CompanyValue {
  id: number;

  titleTr: string;
  titleEn: string;

  descriptionTr: string;
  descriptionEn: string;

  displayOrder: number;
  isPublished: boolean;

  createdAtUtc: string;
  updatedAtUtc: string | null;
}
