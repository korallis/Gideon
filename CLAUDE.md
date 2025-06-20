# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Gideon is a comprehensive desktop application for EVE Online players that serves as an AI Copilot for ship fitting, character planning, and market analysis. The application is built to be highly accurate, with calculations matching EVE Online's server-side formulas within 0.1%.

## Development Environment

This project appears to be in early planning stages with:
- Basic Node.js setup (package-lock.json with minimal dependencies)
- Comprehensive Product Requirements Document (prd.md)
- Development rules and guidelines (devrules.md)

## Key Requirements from PRD

### Core Functionality
- **Ship Fitting Module**: Drag-and-drop interface for creating and optimizing ship fittings
- **Character Management**: ESI API integration for character data synchronization
- **Market Analysis**: Real-time pricing and cost analysis
- **Performance Calculations**: DPS, tank, capacitor, and speed calculations with 0.1% accuracy

### Technical Standards
- **Performance**: Application startup under 5 seconds, UI response under 100ms
- **Memory**: Maximum 500MB during normal operation
- **Accuracy**: Ship fitting calculations within 0.1% of in-game values
- **ESI Integration**: OAuth2 authentication with automatic token refresh

### Development Principles (from devrules.md)
- Use structured, modular, and parallel-friendly code organization
- Maintain comprehensive documentation as code evolves
- Break down complex tasks with chained prompting
- Automate testing and fixing processes

## Architecture Requirements

Based on the PRD, the application should be structured with:
- **Ship Fitting Module**: Core fitting interface and calculations
- **Character Management Module**: ESI authentication and data sync
- **Market Analysis Module**: Price tracking and cost calculations
- **Data Management Module**: Local storage and performance optimization

## Quality Standards

- **Calculation Accuracy**: All EVE game mechanics must be implemented with 100% accuracy
- **Data Integrity**: Zero data loss under normal shutdown procedures
- **Error Handling**: Graceful recovery from all anticipated error conditions
- **Security**: Secure storage of authentication tokens using Windows Credential Manager

## Testing Requirements

- Comprehensive automated testing for all calculations
- ESI API integration testing
- Performance testing for startup time and memory usage
- User acceptance testing with EVE Online community

## Development Plan Integration

### Master Development Tracking
**CRITICAL:** Always check `development-plan.md` before running any command or starting work. This file contains the complete task breakdown and current progress status.

#### Pre-Work Checklist
1. Read `development-plan.md` to identify current active task
2. Check progress tracking section for overall status
3. Review task dependencies and blockers
4. Update task status to "in_progress" when starting work

#### Post-Work Workflow
1. Mark completed tasks as "completed" in development-plan.md
2. Update progress tracking percentages
3. Identify next priority task from the plan
4. Log any blockers or issues encountered
5. Commit changes with reference to task number (e.g., "TASK-001: Initialize Node.js project")

#### Development Approach
- **Frontend-First**: Build complete UI before backend integration
- **Component-Driven**: Create reusable UI components following EVE's sci-fi aesthetic
- **Test-Driven**: Write tests alongside implementation
- **Progressive**: Start with basic features, add complexity incrementally

## Current Status

**Active Development Phase:** Phase 1 - Foundation & Core Infrastructure  
**Current Task:** TASK-001 - Initialize Node.js project  
**Overall Progress:** 0% (0/144 tasks completed)  
**Next Milestone:** Complete project setup and tooling (Tasks 1-9)

The project has comprehensive planning documentation and a detailed 144-task development plan ready for execution.