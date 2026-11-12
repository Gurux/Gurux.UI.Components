# Bundled theme assets

Pinned upstream sources (MIT licenses included beside the CSS):

- Bootstrap 5.3.8: https://registry.npmjs.org/bootstrap/-/bootstrap-5.3.8.tgz
  (`package/dist/css/bootstrap.min.css` to `default.min.css`).
- Bootswatch 5.3.8: https://registry.npmjs.org/bootswatch/-/bootswatch-5.3.8.tgz
  (`package/dist/<theme>/bootstrap.min.css` for all 26 themes).
- Bootstrap documentation: https://getbootstrap.com/docs/5.3/customize/color-modes/
- Bootswatch project: https://github.com/thomaspark/bootswatch

Upstream copyright headers and MIT notices are retained. Remote font `@import`
rules and references to absent source maps were removed. CSS uses font fallbacks
without runtime font requests. Text assets use the repository-required CRLF line
endings. No Bootstrap JavaScript or new package dependency is included.

When updating, select matching Bootstrap and Bootswatch versions, review licenses
and color-mode support, update catalog metadata, and rerun appearance checks.

Font import removal must match the entire `@import url(...)` directive, including
quoted URLs containing semicolons (Google Fonts weight/style lists). Do not stop
at the first semicolon: leftover URL text can invalidate the following :root
selector. The component-style browser matrix verifies that every packaged theme
actually supplies its semantic variables in both modes.
