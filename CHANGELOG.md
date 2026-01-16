# Changelog

## [2.0.0](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0...v2.0.0) (2026-01-16)


### ⚠ BREAKING CHANGES

* updates breaking change to latest jsonpointer.net API

### Features

* Add confirmation prompt before overwriting files with --force/-f flag ([74d70db](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/74d70dbf7f265341c1c75b2275bb6f5b5e4fe966))
* adds 1.1 version ([f0940f5](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/f0940f5e56b444c10d04ce52b2d0ce6b66eb4e48))
* adds a CLI tool to apply overlays ([f1e100d](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/f1e100d479af937e51540d7d65715a4d5888c2bf))
* adds a new normalize command ([f9da1e5](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/f9da1e57437e11ef25950c610f00b40b4105a211))
* adds a result type for apply methods ([47df43e](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/47df43e688a2942855086c9ea01613fe9b598622))
* adds apply methods with high level object model ([342a4f6](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/342a4f6913ac749f389be4ac60542ae3da686703))
* adds basic deserialization infrastructure ([9bcefff](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/9bcefffe7df3851c824bf1bb7a4a17721a31c006))
* adds description info field ([f952f79](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/f952f79afe0b8f388b3c2f464fb504d5033293e3))
* adds deserialization infrastructure for version 1.1 ([495394f](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/495394f08e3661333825fd58b56e57f1c8c218ff))
* adds extensions support ([b4a7d45](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/b4a7d453829a33838ca18a31ceff269f82b3f62b))
* adds info class ([3d563fb](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/3d563fbdce2a713afc40b0b71bf40b0291f93f44))
* adds info parsing ([bfc4930](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/bfc49303150a242a830134de9ed734f167214396))
* adds info serialization implementation ([c8ac3ae](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/c8ac3aef249cd37f00c008146efc83b45e97bdb0))
* adds net 9 and net 10 install support ([496f83b](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/496f83b98d50e0a24ed7655bdf3574284ac8901f))
* adds net 9 and net 10 install support ([12535a4](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/12535a4455f2cf8ebd27e1bac540d583d162b785))
* adds openapi document format detection ([5f6e0c6](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/5f6e0c636812fe487070c2703199ee4d224705a3))
* adds openapi document format detection ([8f1c761](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/8f1c7610a1e48391af203235dbaf5dc01101b5a6))
* adds serialization infrastructure for 1.1 ([659d703](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/659d703c9c595f117c66543dbfc8ea883d4218e3))
* adds serialization of extensions ([0101f3a](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/0101f3a0aa50c41edce1cb8d57e4b0ff4336113a))
* adds split commands for overlay only and overlay with normalization ([07aeaaf](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/07aeaaf432282edd37fad5fb70b2f7ef1610d047))
* adds the ability to apply multiple overlay documents to a single OpenAPI document ([50d6bc9](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/50d6bc96c1887825ebaa270f1a599a7b4f9706a7))
* adds the ability to apply multiple overlay documents to a single OpenAPI document ([d192033](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/d1920331349c756c5e3cf11010b07bf1b25ffa0f))
* adds the ability to apply the overlay to a JSON document ([d580a75](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/d580a75af4ffee0acfa409835e10917583a19c0b))
* adds the ability to apply the overlay to a JSON document ([b4c47d1](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/b4c47d17583b1f0cea2d02760b3bd6367a59b841))
* adds the ability to copy a node into another ([5f86d20](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/5f86d20de56aede1947fda00a8ac40eeb099af4b))
* adds the update field in overlay action ([c43acf0](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/c43acf0a6263b1afc58e17d07fca91dd9fca8205))
* adds yaml parsing reader for overlay ([b790fbc](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/b790fbcf65075039b0ea4b492a9084eb8e44c1b1))
* attempt to go through subsequent actions if one fails ([b73ad65](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/b73ad6545fc2cace549876a2d69664fef98899fb))
* bypass the openapi.net object model when applying overlays to avoid normalization ([0fd0bf0](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/0fd0bf0f2a02e8f5c323a315e267aceed86a06b9))
* removes experimental tag for copy field ([d01b6c0](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/d01b6c0f35324574948497847744f458c182985a))
* switches to a stable version ([fc44f75](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/fc44f7565a6d8afbda0a4cbb72a8ced695379599))
* switches to a stable version ([5dcb10f](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/5dcb10f76f6152b4775d737266a0b3e309560b77))
* Update x-copy behavior to require single source match per specification OAI/Overlay-Specificatino#PR [#150](https://github.com/BinkyLabs/openapi-overlays-dotnet/issues/150) ([#121](https://github.com/BinkyLabs/openapi-overlays-dotnet/issues/121)) ([8263215](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/8263215105cd023bfd5b18e318c46f0a09d31f13))
* upgrades to oai.net GA ([3361d32](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/3361d32e69408e2b7ced571aa755558a57acde30))
* upgrades to oai.net GA ([56afe5b](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/56afe5b493e082d385307c84918a7e7551f36693))


### Bug Fixes

* a bug where array updates would replace and not append additional values ([71c976a](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/71c976ad348c71b28e88475b2b21a2d9f256570b)), closes [#137](https://github.com/BinkyLabs/openapi-overlays-dotnet/issues/137)
* additional conflicting method overloads ([6c61e1c](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/6c61e1c6f63cc9e0477a82262f63d1cc52fff1f9))
* adds a try catch so a single failing action does not crash the whole document application ([cb88170](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/cb88170a20dc2e7db020f6b718bed9f7e91cd27b))
* adds missing unit test ([86b249b](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/86b249b30f10c908abce8b3fe16052a192669b5b))
* adds missing using for the stream ([69549e0](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/69549e0e0330dab64140d5a8011f7d9dccf722ab))
* adds symbols package ([f4a6d3f](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/f4a6d3fdbe9257e94e5a594d92833009f746b27b))
* an issue where navigating to the parent for removal would fail based on the expression ([b2d3b34](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/b2d3b341352ff5056ba109dd90098ef317afa111))
* an issue where the default format for the updated document would always be json instead of the format of the source document ([5034c95](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/5034c95ef8cf437b62bb1c84fda83b8778c1817f))
* an issue where the default format for the updated document would always be json instead of the format of the source document ([d6db167](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/d6db167e7eb4f3cf425c4529d6abfd03e6cd6609))
* applying remove wilcard would fail if other nodes did not have the same property ([46066ed](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/46066ed639107723eafa72ea83e075948edf8af2))
* applying remove wilcard would fail if other nodes did not have the same property ([19489ea](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/19489ea29704ebd5def62e8f38cd948b66b41862))
* avoid stack overflows when applying changes for updates or copies ([3faf8e3](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/3faf8e388563e26b8abc77143b96a902876322d8))
* avoid stack overflows when applying changes for updates or copies ([84c6ee5](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/84c6ee535dda086b79cbc633f9a664da0625e400))
* better error messages when applying overlays through CLI ([9234bdd](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/9234bdd901f0d238ab5eec25d3a8a88782be699c))
* better error messages when applying overlays through CLI ([3bfefd0](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/3bfefd04bc4b2ca5b43fda727f884c68f5782768))
* Bump Microsoft.OpenApi and Microsoft.OpenApi.YamlReader for null serialization in yaml ([835ded8](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/835ded8406f86e53458bdbac78eff9bbd378df9a))
* clean up sync api surface ([3e0813d](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/3e0813d997e4e4ff06a8a9671d773d77d12bb37a))
* do not warn on $schema ([d9d9cec](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/d9d9cecbbc8b54efc73ec4d09c37609fb46b827d))
* do not warn on $schema ([28fdaa9](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/28fdaa9d8a24deb556cc6faba3dffdae655ff72a))
* downgrades target to net8 for broader compatibility ([902293c](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/902293c59f2592365c087ba312ec290ed765cf5f))
* downgrades target to net8 for broader compatibility ([cc2a61a](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/cc2a61a9a0d74686e8795f0433a5c865d6bb4809))
* empty strings should be quoted in yaml ([086c04a](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/086c04a05777c59f3080f1910fac7963c220e879))
* empty strings should be quoted in yaml ([ae22bb7](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/ae22bb789151ffc4b05293277d20c00ee1fa123b))
* extra namespace due to typo ([580ae8f](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/580ae8f5917d9793dd59eab886f4157dbe636898))
* extra namespace due to typo ([86b3262](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/86b32624a4b48521dba79f51da2128646be7d38f))
* if targets for updates are arrays, the item should be inserted ([36bcceb](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/36bccebf75dc3deb20c361774d7c454f621c7db2)), closes [#136](https://github.com/BinkyLabs/openapi-overlays-dotnet/issues/136)
* makes node removal o(n) instead of o(n2) ([bd39370](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/bd39370565231568a1ecb38883b90ae08042b653))
* makes node removal o(n) instead of o(n2) ([c0753a2](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/c0753a27b9139ccfc3d747090d688afd0e1f7359))
* more public api cleanup ([41e4dac](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/41e4dac2bd38cf41d1b01bb3f1f410e4471bfd2b))
* moves OAI document parsing to readers ([b790fbc](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/b790fbcf65075039b0ea4b492a9084eb8e44c1b1))
* relative path management when loading overlay documents ([2d7eb4d](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/2d7eb4de7d70c70d815f186a0245b362870c2cc3))
* removal JSON path buildup contains extra dots leading to failure ([26caaac](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/26caaac292105f24405d86e13da07c1520a3b507))
* removal JSON path buildup contains extra dots leading to failure ([b2bf77c](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/b2bf77cffcec603fa7b2ae0caedff2b431d7c295))
* some of the overload conflicts ([3a6f57d](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/3a6f57d041c36850bdc99070d0e9a4c9321b1c2f))
* unnecessary document parameter in parsing methods ([91d8842](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/91d8842bc87bd77caf3d96f91863312c930a87f9))
* updates breaking change to latest jsonpointer.net API ([70a003c](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/70a003c2d184c97337d48204ab63fcc14b6ff044))
* upgrade oai.net dependency to avoid bad yaml formatting during serialization ([7c4ac46](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/7c4ac46b6794bad206afe85de79a0bac6d1e0b74))
* upgrades dependencies to fix empty default array values ([05214ba](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/05214baa6544618c4b2a0e96a2a0e3de612b9159))
* upgrades dependencies to fix empty default array values ([5f22256](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/5f22256617b2d0fedbcbdba525e7fecf75a7a721))
* use overlay settings in loading and apply methods ([a0a3cae](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/a0a3caeaf622689018261c08d9b15f12d5f8685f))
* yaml OpenAPI descriptions are empty ([1a3ac73](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/1a3ac73d5743b6a9b93331b557795cd780234e87))

## [1.0.0](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.23...v1.0.0) (2026-01-16)


### Bug Fixes

* extra namespace due to typo ([580ae8f](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/580ae8f5917d9793dd59eab886f4157dbe636898))
* extra namespace due to typo ([86b3262](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/86b32624a4b48521dba79f51da2128646be7d38f))

## [1.0.0-preview.22](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.21...v1.0.0-preview.22) (2025-12-15)


### Features

* adds net 9 and net 10 install support ([496f83b](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/496f83b98d50e0a24ed7655bdf3574284ac8901f))
* adds net 9 and net 10 install support ([12535a4](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/12535a4455f2cf8ebd27e1bac540d583d162b785))

## [1.0.0-preview.21](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.20...v1.0.0-preview.21) (2025-12-09)


### ⚠ BREAKING CHANGES

* updates breaking change to latest jsonpointer.net API

### Bug Fixes

* updates breaking change to latest jsonpointer.net API ([70a003c](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/70a003c2d184c97337d48204ab63fcc14b6ff044))

## [1.0.0-preview.20](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.19...v1.0.0-preview.20) (2025-11-17)


### Bug Fixes

* empty strings should be quoted in yaml ([086c04a](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/086c04a05777c59f3080f1910fac7963c220e879))
* empty strings should be quoted in yaml ([ae22bb7](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/ae22bb789151ffc4b05293277d20c00ee1fa123b))

## [1.0.0-preview.19](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.18...v1.0.0-preview.19) (2025-11-10)


### Features

* Add confirmation prompt before overwriting files with --force/-f flag ([74d70db](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/74d70dbf7f265341c1c75b2275bb6f5b5e4fe966))

## [1.0.0-preview.18](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.17...v1.0.0-preview.18) (2025-11-06)


### Bug Fixes

* Bump Microsoft.OpenApi and Microsoft.OpenApi.YamlReader for null serialization in yaml ([835ded8](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/835ded8406f86e53458bdbac78eff9bbd378df9a))

## [1.0.0-preview.17](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.16...v1.0.0-preview.17) (2025-11-05)


### Bug Fixes

* a bug where array updates would replace and not append additional values ([71c976a](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/71c976ad348c71b28e88475b2b21a2d9f256570b)), closes [#137](https://github.com/BinkyLabs/openapi-overlays-dotnet/issues/137)
* if targets for updates are arrays, the item should be inserted ([36bcceb](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/36bccebf75dc3deb20c361774d7c454f621c7db2)), closes [#136](https://github.com/BinkyLabs/openapi-overlays-dotnet/issues/136)
* yaml OpenAPI descriptions are empty ([1a3ac73](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/1a3ac73d5743b6a9b93331b557795cd780234e87))

## [1.0.0-preview.16](https://github.com/BinkyLabs/openapi-overlays-dotnet/compare/v1.0.0-preview.15...v1.0.0-preview.16) (2025-10-27)


### Features

* adds a new normalize command ([f9da1e5](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/f9da1e57437e11ef25950c610f00b40b4105a211))
* adds split commands for overlay only and overlay with normalization ([07aeaaf](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/07aeaaf432282edd37fad5fb70b2f7ef1610d047))
* bypass the openapi.net object model when applying overlays to avoid normalization ([0fd0bf0](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/0fd0bf0f2a02e8f5c323a315e267aceed86a06b9))


### Bug Fixes

* upgrade oai.net dependency to avoid bad yaml formatting during serialization ([7c4ac46](https://github.com/BinkyLabs/openapi-overlays-dotnet/commit/7c4ac46b6794bad206afe85de79a0bac6d1e0b74))

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
