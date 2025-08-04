import { createOpenAPI, attachFile } from "fumadocs-openapi/server";
import { docs } from "@/.source";
import { loader } from "fumadocs-core/source";

// See https://fumadocs.vercel.app/docs/headless/source-api for more info
export const source = loader({
  // it assigns a URL to your pages
  baseUrl: "/docs",
  source: docs.toFumadocsSource(),
  pageTree: {
    // adds a badge to each page item in page tree
    attachFile,
  },
});

export const openapi = createOpenAPI();
