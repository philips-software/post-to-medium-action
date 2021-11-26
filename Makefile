.PHONY: gh-release
gh-release: ## Creates a new release by creating a new tag and pushing it - Example: make gh-release OLD_VERSION=v0.3.1 NEW_VERSION=v0.3.2 DESCRIPTION="Add auto release"
	@git stash -u
	@echo Bumping $(OLD_VERSION) to $(NEW_VERSION)…
	@sed -i 's/$(OLD_VERSION)/$(NEW_VERSION)/g' .github/workflows/*.yml *.yaml *.md
	@git add .
	@git commit -s -m "Bump $(OLD_VERSION) to $(NEW_VERSION) for release"
	@git tag -sam "$(DESCRIPTION)" $(NEW_VERSION)
	@git push origin $(NEW_VERSION)
	@gh release create $(NEW_VERSION) --draft --title "$(DESCRIPTION)"
	@echo
	@echo ATTENTION: MANUAL ACTION REQUIRED!! -- Wait for the release workflow to finish
	@echo
	@echo Check status here https://github.com/philips-software/post-to-medium-action/actions/workflows/environment.yml
	@echo
	@echo Once finished, push the main branch using 'git push'
	@echo
	@echo Visit https://github.com/philips-software/post-to-medium-action/releases
	@echo Edit the release and save it to publish to GitHub Marketplace.
	@echo
	@git stash pop