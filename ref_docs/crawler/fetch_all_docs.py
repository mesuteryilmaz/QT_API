"""
fetch_all_docs.py
=================
Downloads ALL pages from https://api.quantower.com/ and saves them as
clean Markdown files into:
    C:\\Users\\mesuteryilmaz\\Desktop\\QT_DOM_Project\\ref_docs\\api_quantower\\

Usage:
    python fetch_all_docs.py

Requirements:
    pip install requests beautifulsoup4 markdownify

This script:
  1. Starts from the 4 main API category pages
  2. Recursively follows all /docs/* links on api.quantower.com
  3. Converts HTML → clean Markdown
  4. Saves each file with its class name as filename
  5. Creates an INDEX.md listing all downloaded pages
  6. Skips already-downloaded files (safe to re-run)
"""

import os
import re
import time
import requests
from urllib.parse import urljoin, urlparse
from collections import deque

try:
    from bs4 import BeautifulSoup
except ImportError:
    raise SystemExit("Missing: pip install beautifulsoup4")

try:
    from markdownify import markdownify as md
except ImportError:
    raise SystemExit("Missing: pip install markdownify")

# ─── Config ───────────────────────────────────────────────────────────────────

BASE_URL   = "https://api.quantower.com"
START_URLS = [
    "https://api.quantower.com/",
    "https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Core.html",
    "https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Connection.html",
    "https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoricalData.html",
    "https://api.quantower.com/docs/TradingPlatform.BusinessLayer.html",
]

OUTPUT_DIR = r"C:\Users\mesuteryilmaz\Desktop\QT_DOM_Project\ref_docs\api_quantower"
DELAY_SEC  = 0.3   # polite crawl delay between requests

HEADERS = {
    "User-Agent": "Mozilla/5.0 (compatible; QT-DocFetcher/1.0)"
}

# ─── Helpers ──────────────────────────────────────────────────────────────────

def url_to_filename(url: str) -> str:
    """Convert a URL to a safe markdown filename."""
    path = urlparse(url).path
    name = os.path.basename(path)
    if not name or name == "/":
        name = "index"
    name = re.sub(r"\.html$", "", name)
    name = re.sub(r"[^A-Za-z0-9_.\-]", "_", name)
    return name + ".md"


def extract_links(soup, current_url: str) -> list:
    """Extract all internal /docs/ links from the page."""
    links = []
    for a in soup.find_all("a", href=True):
        href = a["href"]
        full = urljoin(current_url, href)
        parsed = urlparse(full)
        # Only follow links on api.quantower.com in the /docs/ path
        if parsed.netloc == "api.quantower.com" and "/docs/" in parsed.path:
            clean = parsed.scheme + "://" + parsed.netloc + parsed.path
            links.append(clean)
    return links


def html_to_markdown(soup) -> str:
    """Extract the main article content and convert to Markdown."""
    # Try to find the main content area
    article = (
        soup.find("article")
        or soup.find("main")
        or soup.find("div", {"id": "main-content"})
        or soup.find("div", {"class": "container"})
        or soup.body
    )
    if article is None:
        article = soup

    # Remove nav, footer, script, style
    for tag in article.find_all(["nav", "footer", "script", "style", "noscript"]):
        tag.decompose()

    raw_md = md(str(article), heading_style="ATX", code_language="csharp")

    # Clean up excessive blank lines
    raw_md = re.sub(r"\n{3,}", "\n\n", raw_md)
    return raw_md.strip()


def fetch_page(url: str) -> tuple:
    """Fetch a URL and return (soup, markdown_text). Returns (None, None) on error."""
    try:
        resp = requests.get(url, headers=HEADERS, timeout=15)
        resp.raise_for_status()
        soup = BeautifulSoup(resp.text, "html.parser")
        title = soup.title.string.strip() if soup.title else url
        markdown = f"# {title}\n> Source: {url}\n> Fetched: {time.strftime('%Y-%m-%d')}\n\n---\n\n"
        markdown += html_to_markdown(soup)
        return soup, markdown
    except Exception as exc:
        print(f"  [WARN] Failed to fetch {url}: {exc}")
        return None, None


# ─── Main Crawler ─────────────────────────────────────────────────────────────

def main():
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    print(f"Output directory: {OUTPUT_DIR}\n")

    visited    = set()
    queue      = deque(START_URLS)
    downloaded = []   # (url, filename) tuples

    while queue:
        url = queue.popleft()
        if url in visited:
            continue
        visited.add(url)

        filename = url_to_filename(url)
        filepath = os.path.join(OUTPUT_DIR, filename)

        if os.path.exists(filepath):
            print(f"  [SKIP] {filename}  (already exists)")
            downloaded.append((url, filename))
            # Still parse for links
            with open(filepath, "r", encoding="utf-8") as f:
                pass  # can't re-parse markdown for links, skip
            continue

        print(f"  [FETCH] {url}")
        soup, markdown = fetch_page(url)

        if soup is None:
            continue

        # Write file
        with open(filepath, "w", encoding="utf-8") as f:
            f.write(markdown)
        downloaded.append((url, filename))
        print(f"    → saved: {filename}")

        # Discover new links
        new_links = extract_links(soup, url)
        for link in new_links:
            if link not in visited:
                queue.append(link)

        time.sleep(DELAY_SEC)

    # ─── Write INDEX ──────────────────────────────────────────────────────────
    index_path = os.path.join(OUTPUT_DIR, "00_INDEX.md")
    with open(index_path, "w", encoding="utf-8") as f:
        f.write("# Quantower API Reference — Full Index\n")
        f.write(f"> Downloaded: {time.strftime('%Y-%m-%d %H:%M')}\n")
        f.write(f"> Total pages: {len(downloaded)}\n\n")
        f.write("---\n\n")
        for url, fn in sorted(downloaded, key=lambda x: x[1]):
            f.write(f"- [{fn}]({fn})  \n  `{url}`\n")

    print(f"\n✓ Done. {len(downloaded)} pages saved to:\n  {OUTPUT_DIR}")
    print(f"  Index: {index_path}")


if __name__ == "__main__":
    main()
