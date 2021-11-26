.PHONY: gh-release
gh-release: ## Creates a new release by creating a new tag and pushing it - Example: make gh-release OLD_VERSION=v0.3.1 NEW_VERSION=v0.3.2 DESCRIPTION="Add auto release"
	@git stash -u
	@echo Bumping $(OLD_VERSION) to $(NEW_VERSION)…
	@sed -i 's/$(OLD_VERSION)/$(NEW_VERSION)/g' Action/*.csproj .github/workflows/*.yaml *.yaml *.md
	@git add .
	@git commit -s -m "Bump $(OLD_VERSION) to $(NEW_VERSION) for release"
	@git tag -sam "$(DESCRIPTION)" $(NEW_VERSION)
	@git push origin $(NEW_VERSION)
	@git stash pop
	@gh release create $(NEW_VERSION) --draft