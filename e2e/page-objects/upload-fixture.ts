import { StudioFixture } from "./studio-fixture";

export function uploadFixture() {
  const fixture = new StudioFixture();
  fixture.records["sessions"] = [
    {
      id: "session-a",
      version: 1,
      name: "Portrait session",
      clientIds: [],
      service: "Headshot",
    },
  ];
  let batch: any;
  fixture.operations.set(
    "upload.create",
    (sessionId, files) =>
      (batch = {
        id: "batch-a",
        sessionId,
        files: files.map((file: any, index: number) => ({
          ...file,
          photoId:
            /\.(jpg|jpeg|cr2|cr3|nef|arw|dng)$/i.test(file.name) &&
            /^[a-f0-9]{64}$/i.test(file.sha256)
              ? `photo-${index}`
              : null,
          state: "Uploading",
          rejection: /\.txt$/i.test(file.name) ? "Unsupported format." : null,
        })),
      }),
  );
  fixture.operations.set("upload.status", () => batch);
  fixture.operations.set("upload.renew", () => ({
    url: "https://controlled-storage.invalid/upload",
    expiresAt: "2099-01-01T00:00:00Z",
  }));
  fixture.operations.set("upload.block", () => undefined);
  fixture.operations.set("upload.commit", () => undefined);
  fixture.operations.set("upload.complete", (_batch, photoId) => {
    batch.files.find((file: any) => file.photoId === photoId).state = "Ready";
    return { state: "Ready", failure: null };
  });
  return fixture;
}
