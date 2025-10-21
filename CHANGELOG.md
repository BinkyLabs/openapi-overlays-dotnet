# Changelog

## [1.0.0-preview.15](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.14...v1.0.0-preview.15) (2025-10-21)


### Features

* Update x-copy behavior to require single source match per specification OAI/Overlay-Specificatino#PR [#150](https://github.com/BinkyLabs/openapi-overlays-dotnet/issues/150) ([#121](https://github.com/BinkyLabs/openapi-overlays-dotnet/issues/121)) ([8263215](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/8263215105cd023bfd5b18e318c46f0a09d31f13))

## [1.0.0-preview.14](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.13...v1.0.0-preview.14) (2025-10-20)


### Bug Fixes

* upgrades dependencies to fix empty default array values ([05214ba](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/05214baa6544618c4b2a0e96a2a0e3de612b9159))
* upgrades dependencies to fix empty default array values ([5f22256](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/5f22256617b2d0fedbcbdba525e7fecf75a7a721))

## [1.0.0-preview.13](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.12...v1.0.0-preview.13) (2025-10-17)


### Bug Fixes

* an issue where navigating to the parent for removal would fail based on the expression ([b2d3b34](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/b2d3b341352ff5056ba109dd90098ef317afa111))

## [1.0.0-preview.12](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.11...v1.0.0-preview.12) (2025-10-17)


### Features

* adds a result type for apply methods ([47df43e](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/47df43e688a2942855086c9ea01613fe9b598622))
* attempt to go through subsequent actions if one fails ([b73ad65](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/b73ad6545fc2cace549876a2d69664fef98899fb))


### Bug Fixes

* adds a try catch so a single failing action does not crash the whole document application ([cb88170](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/cb88170a20dc2e7db020f6b718bed9f7e91cd27b))
* an issue where the default format for the updated document would always be json instead of the format of the source document ([5034c95](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/5034c95ef8cf437b62bb1c84fda83b8778c1817f))
* an issue where the default format for the updated document would always be json instead of the format of the source document ([d6db167](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/d6db167e7eb4f3cf425c4529d6abfd03e6cd6609))
* avoid stack overflows when applying changes for updates or copies ([3faf8e3](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/3faf8e388563e26b8abc77143b96a902876322d8))
* avoid stack overflows when applying changes for updates or copies ([84c6ee5](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/84c6ee535dda086b79cbc633f9a664da0625e400))

## [1.0.0-preview.11](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.10...v1.0.0-preview.11) (2025-10-16)


### Features

* adds the ability to copy a node into another ([5f86d20](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/5f86d20de56aede1947fda00a8ac40eeb099af4b))

## [1.0.0-preview.10](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.9...v1.0.0-preview.10) (2025-08-30)


### Bug Fixes

* relative path management when loading overlay documents ([2d7eb4d](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/2d7eb4de7d70c70d815f186a0245b362870c2cc3))

## [1.0.0-preview.9](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.8...v1.0.0-preview.9) (2025-08-29)


### Bug Fixes

* applying remove wilcard would fail if other nodes did not have the same property ([46066ed](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/46066ed639107723eafa72ea83e075948edf8af2))
* applying remove wilcard would fail if other nodes did not have the same property ([19489ea](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/19489ea29704ebd5def62e8f38cd948b66b41862))
* better error messages when applying overlays through CLI ([9234bdd](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/9234bdd901f0d238ab5eec25d3a8a88782be699c))
* better error messages when applying overlays through CLI ([3bfefd0](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/3bfefd04bc4b2ca5b43fda727f884c68f5782768))
* do not warn on $schema ([d9d9cec](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/d9d9cecbbc8b54efc73ec4d09c37609fb46b827d))
* do not warn on $schema ([28fdaa9](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/28fdaa9d8a24deb556cc6faba3dffdae655ff72a))
* removal JSON path buildup contains extra dots leading to failure ([26caaac](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/26caaac292105f24405d86e13da07c1520a3b507))
* removal JSON path buildup contains extra dots leading to failure ([b2bf77c](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/b2bf77cffcec603fa7b2ae0caedff2b431d7c295))

## [1.0.0-preview.8](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.7...v1.0.0-preview.8) (2025-08-29)


### Features

* adds a CLI tool to apply overlays ([f1e100d](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/f1e100d479af937e51540d7d65715a4d5888c2bf))


### Bug Fixes

* adds missing unit test ([86b249b](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/86b249b30f10c908abce8b3fe16052a192669b5b))

## [1.0.0-preview.7](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.6...v1.0.0-preview.7) (2025-08-16)


### Bug Fixes

* adds missing using for the stream ([69549e0](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/69549e0e0330dab64140d5a8011f7d9dccf722ab))

## [1.0.0-preview.6](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.5...v1.0.0-preview.6) (2025-08-09)


### Features

* adds the ability to apply multiple overlay documents to a single OpenAPI document ([50d6bc9](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/50d6bc96c1887825ebaa270f1a599a7b4f9706a7))
* adds the ability to apply multiple overlay documents to a single OpenAPI document ([d192033](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/d1920331349c756c5e3cf11010b07bf1b25ffa0f))

## [1.0.0-preview.5](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.4...v1.0.0-preview.5) (2025-08-04)


### Features

* upgrades to oai.net GA ([3361d32](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/3361d32e69408e2b7ced571aa755558a57acde30))
* upgrades to oai.net GA ([56afe5b](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/56afe5b493e082d385307c84918a7e7551f36693))

## [1.0.0-preview.4](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.3...v1.0.0-preview.4) (2025-07-06)


### Bug Fixes

* downgrades target to net8 for broader compatibility ([902293c](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/902293c59f2592365c087ba312ec290ed765cf5f))
* downgrades target to net8 for broader compatibility ([cc2a61a](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/cc2a61a9a0d74686e8795f0433a5c865d6bb4809))

## [1.0.0-preview.3](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.2...v1.0.0-preview.3) (2025-07-06)


### Bug Fixes

* adds symbols package ([f4a6d3f](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/f4a6d3fdbe9257e94e5a594d92833009f746b27b))

## [1.0.0-preview.2](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.1...v1.0.0-preview.2) (2025-07-06)


### Features

* adds apply methods with high level object model ([342a4f6](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/342a4f6913ac749f389be4ac60542ae3da686703))
* adds basic deserialization infrastructure ([9bcefff](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/9bcefffe7df3851c824bf1bb7a4a17721a31c006))
* adds extensions support ([b4a7d45](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/b4a7d453829a33838ca18a31ceff269f82b3f62b))
* adds info class ([3d563fb](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/3d563fbdce2a713afc40b0b71bf40b0291f93f44))
* adds info parsing ([bfc4930](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/bfc49303150a242a830134de9ed734f167214396))
* adds info serialization implementation ([c8ac3ae](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/c8ac3aef249cd37f00c008146efc83b45e97bdb0))
* adds openapi document format detection ([5f6e0c6](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/5f6e0c636812fe487070c2703199ee4d224705a3))
* adds openapi document format detection ([8f1c761](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/8f1c7610a1e48391af203235dbaf5dc01101b5a6))
* adds serialization of extensions ([0101f3a](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/0101f3a0aa50c41edce1cb8d57e4b0ff4336113a))
* adds the ability to apply the overlay to a JSON document ([d580a75](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/d580a75af4ffee0acfa409835e10917583a19c0b))
* adds the ability to apply the overlay to a JSON document ([b4c47d1](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/b4c47d17583b1f0cea2d02760b3bd6367a59b841))
* adds the update field in overlay action ([c43acf0](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/c43acf0a6263b1afc58e17d07fca91dd9fca8205))


### Bug Fixes

* makes node removal o(n) instead of o(n2) ([bd39370](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/bd39370565231568a1ecb38883b90ae08042b653))
* makes node removal o(n) instead of o(n2) ([c0753a2](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/c0753a27b9139ccfc3d747090d688afd0e1f7359))

## Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
